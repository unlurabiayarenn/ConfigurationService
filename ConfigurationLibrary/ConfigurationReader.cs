using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;

namespace ConfigurationLibrary
{
	public class ConfigurationReader
	{
		private readonly string _applicationName;
		private readonly string _connectionString;
		private readonly int _refreshTimerIntervalInMs;
		private Dictionary<string, object> _configurations;
		private Timer _timer;

		public ConfigurationReader(string applicationName, string connectionString, int refreshTimerIntervalInMs)
		{
			_applicationName = applicationName;
			_connectionString = connectionString;
			_refreshTimerIntervalInMs = refreshTimerIntervalInMs;
			_configurations = new Dictionary<string, object>();
			Initialize();
		}

		private void Initialize()
		{
			LoadConfigurations();
			_timer = new Timer(RefreshConfigurations, null, _refreshTimerIntervalInMs, _refreshTimerIntervalInMs);
		}

		public Dictionary<string, object> GetAllConfigurations()
		{
			return _configurations;
		}


		private void LoadConfigurations(){
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				var command = new SqlCommand("SELECT Key, Value, Type FROM Configurations WHERE ApplicationName = @ApplicationName AND IsActive = 1", connection);
				command.Parameters.AddWithValue("@ApplicationName", _applicationName);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var key = reader["Key"].ToString();
						var value = reader["Value"].ToString();
						var type = reader["Type"].ToString();

						_configurations[key] = ConvertToType(value, type);

					}
				}
			}
		}

		public void RefreshConfigurations(object state)
		{
			LoadConfigurations();
		}

		private object ConvertToType(string value, string type)
		{
			switch (type.ToLower())
			{
				case "int":
				case "integer":
					return int.Parse(value);
				case "bool":
				case "boolean":
					return bool.Parse(value);
				default:
					return value;
			}
		}

		public T GetValue<T>(string key)
		{
			if (_configurations.TryGetValue(key, out var value))
			{
				return (T)value;
			}
			throw new KeyNotFoundException($"Key '{key}' not found.");
		}
	}
}

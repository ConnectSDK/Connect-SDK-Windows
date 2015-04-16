#region Copyright Notice
/*
 * ConnectSdk.Windows
 * Storage.cs
 * 
 * Copyright (c) 2015 LG Electronics.
 * Created by Sorin S. Serban on 20-2-2015,
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 #endregion
using System;

namespace ConnectSdk.Windows.Etc.Helper
{
    public class Storage
    {
        private readonly global::Windows.Storage.ApplicationData settings;
        public const string StoredDevicesKeyName = "StoredDevices";
        public const string StoredKeysKeyName = "StoredKeys";
        public const string StoredVibrationKeyName = "VibrationSetting";

        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        public Storage()
        {
            // Get the settings for this application.
            //settings = IsolatedStorageSettings.ApplicationSettings;
            settings = global::Windows.Storage.ApplicationData.Current;
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddOrUpdateValue(string key, Object value)
        {
            bool valueChanged;

            // If the key exists
            if (settings.LocalSettings.Values.ContainsKey(key))
            {
                // If the value has changed
                if (settings.LocalSettings.Values[key] == value) return false;
                // Store the new value
                try
                {
                    settings.LocalSettings.Values[key] = value;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                        
                }
            }
                // Otherwise create the key.
            else
            {
                settings.LocalSettings.Values.Add(key, value);
            }
            return true;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValueOrDefault<T>(string key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (settings.LocalSettings.Values.ContainsKey(key))
            {
                value = (T)settings.LocalSettings.Values[key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Save the settings.
        /// </summary>
        public void Save()
        {
            //settings.Save();
        }


        private static Storage current;

        public static Storage Current
        {
            get { return current ?? (current = new Storage()); }
        }

        public string StoredDevices
        {
            get
            {
                return GetValueOrDefault(StoredKeysKeyName, ""); 
            }
        }
    }
}


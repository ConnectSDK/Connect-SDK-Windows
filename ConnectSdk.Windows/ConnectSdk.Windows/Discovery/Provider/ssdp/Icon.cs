#region Copyright Notice
/*
 * ConnectSdk.Windows
 * Icon.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 22-4-2015,
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
namespace ConnectSdk.Windows.Discovery.Provider.ssdp
{
    public class Icon 
    {
        public static string Tag = "icon";
        public static string TagMimeType = "mimetype";
        public static string TagWidth = "width";
        public static string TagHeight = "height";
        public static string TagDepth = "depth";
        public static string TagUrl = "url";
	
        /// <summary>
        /// Required. Icon's MIME type.
        /// </summary>
        public string MimeType;

        /// <summary>
        /// Required. Horizontal dimension of icon in pixels.
        /// </summary>
        public string Width;

        /// <summary>
        /// Required. Vertical dimension of icon in pixels.
        /// </summary>
        public string Height;

        /// <summary>
        /// Required. Number of color bits per pixel. 
        /// </summary>
        public string Depth;

        /// <summary>
        /// Required. Pointer to icon image.
        /// </summary>
        public string Url;
    }
}
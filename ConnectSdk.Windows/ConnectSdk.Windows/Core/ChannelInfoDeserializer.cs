#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ChannelInfoDeserializer.cs
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

namespace ConnectSdk.Windows.Core
{
    /// <summary>
    /// These classes are generated from the xml sent from the NetCastTV and will be used for deserialization of the channel list
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class envelope
    {
        private envelopeDataList dataListField;
        /// 
        public envelopeDataList dataList
        {
            get
            {
                return this.dataListField;
            }
            set
            {
                this.dataListField = value;
            }
        }
    }
    /// 
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class envelopeDataList
    {
        private envelopeDataListData[] dataField;
        private string nameField;
        /// 
        [System.Xml.Serialization.XmlElementAttribute("data")]
        public envelopeDataListData[] data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }
    /// 
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class envelopeDataListData
    {
        private string chtypeField;
        private sbyte sourceIndexField;
        private sbyte physicalNumField;
        private short majorField;
        private short displayMajorField;
        private sbyte minorField;
        private sbyte displayMinorField;
        private string chnameField;
        /// 
        public string chtype
        {
            get
            {
                return this.chtypeField;
            }
            set
            {
                this.chtypeField = value;
            }
        }
        /// 
        public sbyte sourceIndex
        {
            get
            {
                return this.sourceIndexField;
            }
            set
            {
                this.sourceIndexField = value;
            }
        }
        /// 
        public sbyte physicalNum
        {
            get
            {
                return this.physicalNumField;
            }
            set
            {
                this.physicalNumField = value;
            }
        }
        /// 
        public short major
        {
            get
            {
                return this.majorField;
            }
            set
            {
                this.majorField = value;
            }
        }
        /// 
        public short displayMajor
        {
            get
            {
                return this.displayMajorField;
            }
            set
            {
                this.displayMajorField = value;
            }
        }
        /// 
        public sbyte minor
        {
            get
            {
                return this.minorField;
            }
            set
            {
                this.minorField = value;
            }
        }
        /// 
        public sbyte displayMinor
        {
            get
            {
                return this.displayMinorField;
            }
            set
            {
                this.displayMinorField = value;
            }
        }
        /// 
        public string chname
        {
            get
            {
                return this.chnameField;
            }
            set
            {
                this.chnameField = value;
            }
        }
    }
}

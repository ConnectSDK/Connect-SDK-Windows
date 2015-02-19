
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

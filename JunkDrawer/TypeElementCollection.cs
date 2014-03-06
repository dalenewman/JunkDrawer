using System;
using System.Configuration;

namespace JunkDrawer {
    public class TypeElementCollection : ConfigurationElementCollection {

        private const string TOP = "top";
        private const string SAMPLE = "sample";
        private const string DEFAULT = "default";
        private const string DEFAULT_LENGTH = "default-length";

        [ConfigurationProperty(TOP, IsRequired = false, DefaultValue = 0)]
        public int Top {
            get { return (int) this[TOP]; }
            set { this[TOP] = value; }
        }

        [ConfigurationProperty(SAMPLE, IsRequired = false, DefaultValue = "100")]
        public decimal Sample {
            get { return Convert.ToDecimal(this[SAMPLE]); }
            set { this[SAMPLE] = value; }
        }

        [ConfigurationProperty(DEFAULT, IsRequired = false, DefaultValue = "string")]
        public string Default {
            get { return this[DEFAULT] as string; }
            set { this[DEFAULT] = value; }
        }

        [ConfigurationProperty(DEFAULT_LENGTH, IsRequired = false, DefaultValue = "1024")]
        public string DefaultLength {
            get { return this[DEFAULT_LENGTH] as string; }
            set { this[DEFAULT_LENGTH] = value; }
        }

        public TypeConfigurationElement this[int index] {
            get { return BaseGet(index) as TypeConfigurationElement; }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public override bool IsReadOnly() {
            return false;
        }

        protected override ConfigurationElement CreateNewElement() {
            return new TypeConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((TypeConfigurationElement)element).Type.ToLower();
        }

        public void Add(TypeConfigurationElement element) {
            BaseAdd(element);
        }
    }
}
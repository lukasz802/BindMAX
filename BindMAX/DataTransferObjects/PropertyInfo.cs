using BindMAX.Enums;

namespace BindMAX.DataTransferObjects
{
    internal class PropertyInfo
    {
        #region Properties

        public bool IsSharedProperty { get; set; }

        public BindingType? BindingType { get; set; }

        public object PropertyValue { get; set; }

        #endregion
    }
}

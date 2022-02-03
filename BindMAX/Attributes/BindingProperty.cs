using BindMAX.Enums;
using System;

namespace BindMAX.Attributes
{

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class BindingProperty : Attribute
    {
        #region Properties

        public BindingType BindingType { get; }

        #endregion

        #region Constructors

        public BindingProperty(BindingType bindingType)
        {
            this.BindingType = bindingType;
        }

        public BindingProperty()
        {
            this.BindingType = BindingType.TwoWays;
        }

        #endregion
    }
}

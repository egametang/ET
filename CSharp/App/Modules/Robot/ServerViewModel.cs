using System.Runtime.Serialization;
using Microsoft.Practices.Prism.Mvvm;

namespace Robot
{
    [DataContract]
    public class ServerViewModel: BindableBase
    {
        [DataMember(Order = 1, IsRequired = true)]
        private string name;

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.SetProperty(ref this.name, value);
            }
        }
    }
}
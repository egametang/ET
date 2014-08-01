using System.ComponentModel.Composition;

namespace Tree
{
    [Export(contractType: typeof(TreeInfoViewModel)), PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
    internal class TreeInfoViewModel
    {
        private int id;
        private string comment;

        public TreeInfoViewModel(int id, string comment)
        {
            this.id = id;
            this.comment = comment;
        }

        public int Id
        {
            get
            {
                return this.id;
            }
        }

        public string Comment
        {
            get
            {
                return this.comment;
            }
        }
    }
}
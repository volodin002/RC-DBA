using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Metamodel
{
    public class SequenceAttribute : Attribute
    {
        private string _sequenceName;
        public SequenceAttribute(string sequenceName)
        {
            _sequenceName = sequenceName;
        }

        
        public string SequenceName { get => _sequenceName; }
    }
}

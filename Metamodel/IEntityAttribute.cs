using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RC.DBA.Metamodel
{
    public interface IEntityAttribute
    {
        string Name { get; }
        MemberInfo Member { get; }

        MethodInfo GetGetter();

        Type MemberType { get; }

        IEntityType EntityType { get; }

        bool IsCollection { get; }

        string Column { get; set; }

        bool IsAssociation { get; }

        bool IsRequired { get; set; }

        bool IsProperty { get; }

        bool IsKey { get; set; }

        bool IsNotMapped { get; set; }

        int MaxLength { get; set; }

        DatabaseGeneratedOption DbGeneratedOption { get; set; }

        string KeySequenceName { get; set; } 

        IEntityAttribute ForeignKey { get; set; }

        IEntityAttribute GetCollectionForeignKey();
    }
}

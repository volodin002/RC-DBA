using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Metamodel
{
    public enum PersistenceType
    {
        ENTITY, EMBEDDABLE, MAPPED_SUPERCLASS, BASIC
    }
    public interface IType<X>
    {
        PersistenceType getPersistenceType();

        Type getNetType();
    }

    public enum BindableType
    {
        SINGULAR_ATTRIBUTE, PLURAL_ATTRIBUTE, ENTITY_TYPE
    }
    public interface IBindable<T>
    {
        BindableType getBindableType();

        Type getBindableNetType();
    }
}

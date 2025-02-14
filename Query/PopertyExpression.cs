using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.DBA.Metamodel;

namespace RC.DBA.Query
{
    public class PopertyExpression : IPopertyExpression
    {
        protected IAliasExpression _alias;
        protected IEntityAttribute _entityAttribute;

        public PopertyExpression(IAliasExpression alias, IEntityAttribute entityAttribute)
        {
            _alias = alias;
            _entityAttribute = entityAttribute;
        }

        public StringBuilder CompileToSQL(StringBuilder sql)
        {
            return _entityAttribute.EntityType.Table.CompileAlias(sql, _alias)
                .Append('.').Append(_entityAttribute.Column);
        }
    }
}

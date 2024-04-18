using PocketMapperORM.DatabaseObjects.Constraints;
using PocketMapperORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketMapperORM.Abstracts
{
    public abstract class Table<SELF>
        where SELF : Table<SELF>
    {
        public Type TypeOfRepresentedEntity { get; set; }
        public string TableName { get; set; }
        public IColumnInfo PrimaryKey { get; set; }
        public IList<IColumnInfo> Columns { get; set; }
        public PrimaryKeyConstraint<SELF> PrimaryKeyConstraint
        {
            get
            {
                return PrimaryKeyConstraint<SELF>.CreateConstraint(PrimaryKey, (SELF)this);
            }
        }
        public IList<ForeignKeyConstraint<SELF>> ForeignKeyConstraints { get; private set; }
        public Table()
        {
            Columns = new List<IColumnInfo>();
            ForeignKeyConstraints = new List<ForeignKeyConstraint<SELF>>();
        }
        public abstract string GenerateCreateTableCommand();
        public abstract string GenerateCreateTableCommandNoFKContsraints();
        public abstract string GenerateAddFKConstraintCommand(ForeignKeyConstraint<SELF> fkConstraint);
    }
}

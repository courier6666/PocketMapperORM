using PocketMapperORM.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PocketMapperORM.DatabaseObjects.Constraints;

namespace PocketMapperORM.DatabaseObjects.Tables
{
    public class SqlServerTable : Table<SqlServerTable>
    {
        public override string GenerateCreateTableCommand()
        {
            return $"""
                CREATE TABLE {this.TableName}
                (
                    {$"{this.PrimaryKey.Name} {this.PrimaryKey.DataType} {"PRIMARY KEY"} {(this.PrimaryKey.isAutoIncremented ? "IDENTITY (1,1)" : "")}"},
                    {string.Join(", \n", this.Columns.Select(c => $"{c.Name} {c.DataType} {(c.IsNullable ? "" : @"NOT NULL")} {(c.isAutoIncremented ? "IDENTITY (1,1)" : "")}").ToArray())},
                    {string.Join(", \n", this.
                            ForeignKeyConstraints.
                            Select(fk => $"CONSTRAINT FK_{fk.ReferencedTableName}{this.TableName} FOREIGN KEY ({fk.ForeignKeyColumn.Name}) REFERENCES {fk.ReferencedTableName}({fk.ReferencedColumn.Name})").ToArray())}
                );
                """;
        }
        public override string GenerateCreateTableCommandNoFKContsraints()
        {
            return $"""
                    CREATE TABLE {this.TableName}
                    (
                        {$"{this.PrimaryKey.Name} {this.PrimaryKey.DataType} {"PRIMARY KEY"} {(this.PrimaryKey.isAutoIncremented ? "IDENTITY (1,1)" : "")}"},
                        {string.Join(", \n", this.Columns.Select(c => $"{c.Name} {c.DataType} {(c.IsNullable ? "" : @"NOT NULL")} {(c.isAutoIncremented ? "IDENTITY (1,1)" : "")}").ToArray())}
                    );
                    """;
        }
        public override string GenerateAddFKConstraintCommand(ForeignKeyConstraint<SqlServerTable> fkConstraint)
        {
            return $"""
                ALTER TABLE {this.TableName}
                ADD FOREIGN KEY ({fkConstraint.ForeignKeyColumn.Name}) REFERENCES {fkConstraint.ReferencedTable.TableName}({fkConstraint.ReferencedColumn.Name});
            """;
        }
    }
}

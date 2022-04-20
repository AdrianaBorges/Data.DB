using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Data.Base
{
    public abstract class BaseDao<T> : DB
    {
        public List<T> GetAll(IStoredProcedureContext context)
        {
            var entidades = new List<T>();

            using (var command = GetCommand(context.NAME))
            {
                command.CommandType = CommandType.StoredProcedure;
                context.AddParameters(command);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        entidades.Add(Hydrate(reader));
                    }
                }
            }

            return entidades;
        }

        public List<T> GetAll()
        {
            var entidades = new List<T>();

            using (var command = GetCommand(GetSelectCommand()))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        entidades.Add(Hydrate(reader));
                    }
                }
            }

            return entidades;
        }

        public List<T> GetAll(string foreignKey)
        {
            var entidades = new List<T>();

            using (var command = GetCommand(GetSelectCommandWithJoin(foreignKey)))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        entidades.Add(Hydrate(reader));
                    }
                }
            }

            return entidades;
        }

        protected T GetBySql(string sql)
        {
            T entidade = default(T);

            using (var reader = GetDataReader(sql))
            {
                while (reader.Read())
                {
                    entidade = Hydrate(reader);
                }
            }

            return entidade;
        }

        public T Get(IStoredProcedureContext context)
        {
            T entidade = default(T);

            using (var command = GetCommand(context.NAME))
            {
                command.CommandType = CommandType.StoredProcedure;
                context.AddParameters(command);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        entidade = Hydrate(reader);
                    }
                }
            }

            return entidade;
        }

        public T Get(string id)
        {
            T entidade = default(T);

            using (var reader = GetDataReader(GetSelectCommand(id)))
            {
                while (reader.Read())
                {
                    entidade = Hydrate(reader);
                }
            }

            return entidade;
        }

        public bool Exists(T entity)
        {
            return ExistsValue(GetExistsCommand(entity));
        }

        public void Insert(T entity)
        {
            Execute(GetInsertCommand(entity));
        }

        public void Delete(T entity)
        {
            Execute(GetDeleteCommand(entity));
        }

        public void Update(T entity)
        {
            Execute(GetUpdateCommand(entity));
        }

        public void DeleteAll(List<T> entitys)
        {
            foreach (var item in entitys)
                Delete(item);
        }

        public void SaveAll(List<T> entitys)
        {
            foreach (var item in entitys)
                Insert(item);
        }

        public void SaveIfNotExists(List<T> entitys)
        {
            foreach (var item in entitys)
            {
                if (!Exists(item))
                    Insert(item);
            }
        }

        public void SaveOrUpdateIfExists(List<T> entitys)
        {
            foreach (var item in entitys)
            {
                if (Exists(item))
                    Update(item);
                else
                    Insert(item);
            }
        }

        protected abstract string GetSelectCommand();
        protected abstract string GetSelectCommand(string id);
        protected virtual string GetSelectCommand(string login, string senha)
        {
            return "";
        }
        protected abstract string GetSelectCommandWithJoin(string foreignKey);
        protected abstract string GetInsertCommand(T entidade);
        protected abstract string GetDeleteCommand(T entidade);
        protected abstract string GetUpdateCommand(T entidade);
        protected abstract string GetExistsCommand(T entidade);
        protected abstract T Hydrate(SqlDataReader reader);
    }
}

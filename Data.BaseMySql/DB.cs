using System;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace Data.BaseMySql
{
    public class ExcecaoAbrirConexao : Exception
    {
        public ExcecaoAbrirConexao(string mensagem)
            : base(mensagem)
        {
        }
    }

    public class ExcecaoFecharConexao : Exception
    {
        public ExcecaoFecharConexao(string mensagem)
            : base(mensagem)
        {
        }
    }

    public class ExcecaoExecutarSQL : Exception
    {
        public ExcecaoExecutarSQL(string mensagem)
            : base(mensagem)
        {
        }
    }

    public class DB
    {
        private MySqlConnection _connection;

        /// <summary>
        /// Obtém a string de conexão do arquivo App.config.
        /// </summary>
        private string GetConnectionString()
        {
            try
            {
                return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            }
            catch (Exception ex)
            {
                throw new Exception("Descrição: Erro ao obter a string de conexão: \"ConnectionString\"." + "\n" +
                                    "Verifique se o arquivo App.config possui uma string de conexão como esta:" + "\n" +
                                    "<connectionStrings>" + "\n" +
                                    "<add name=\"ConnectionString\"" + "\n" +
                                    "connectionString=\"server=localhost;User Id=root;Persist Security Info=True;database=test;uid=root;pwd=root;\"/>" + "\n" +
                                    "</connectionStrings>" + "\n" + "Erro : " + ex.Message);
            }
        }

        /// <summary>
        /// Abre a conexão com o banco de dados.
        /// </summary>
        public void OpenConnection()
        {
            try
            {
                if (_connection == null || _connection.ConnectionString == "")
                {
                    _connection = new MySqlConnection(GetConnectionString());
                }

                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                    return;
                }

                throw new Exception("Conexão não estava fechada.");
            }
            catch (Exception ex)
            {
                throw new ExcecaoAbrirConexao("Descrição: Erro ao abrir conexão." + "\n" + "Erro : " + ex.Message);
            }
        }

        /// <summary>
        /// Fecha a conexão com o banco de dados.
        /// </summary>
        public void CloseConnection()
        {
            try
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                    _connection.Dispose();
                    return;
                }

                throw new Exception("Conexão não estava aberta.");
            }
            catch (Exception ex)
            {
                throw new ExcecaoFecharConexao("Descrição: Erro ao fechar conexão." + "\n" + "Erro : " + ex.Message);
            }
        }

        /// <summary>
        /// Retorna um SqlCommand baseado no texto da query passada pelo parâmetro command.
        /// </summary>
        /// <param name="command">Query ou nome de algum procedimento armazenado no banco.</param>
        public MySqlCommand GetCommand(string command)
        {
            return new MySqlCommand(command, _connection);
        }

        /// <summary>
        /// Retorna true se a query passado pelo parâmetro sql retornar uma linha.
        /// </summary>
        /// <param name="sql">Query</param>
        public bool ExistsValue(string sql)
        {
            try
            {
                bool exists;

                using (var command = GetCommand(sql))
                {
                    using (var dataReader = command.ExecuteReader())
                    {
                        exists = dataReader.HasRows;
                        dataReader.Close();
                    }
                }

                return exists;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (MySqlException ex)
            {
                throw new ExcecaoExecutarSQL(ex.Message);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        /// <summary>
        /// Retorna o número de linhas afetadas pela Stored Procedure passado pelo parâmetro context.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa IStoredProcedureContext</param>
        public int Execute(IStoredProcedureContext context)
        {
            try
            {
                int rows;

                using (var command = GetCommand(context.NAME))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    context.AddParameters(command);

                    rows = command.ExecuteNonQuery();
                }

                return rows;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (MySqlException ex)
            {
                throw new ExcecaoExecutarSQL(ex.Message);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        /// <summary>
        /// Retorna o número de linhas afetadas pela query passada por parâmetro.
        /// </summary>
        /// <param name="sql">Query</param>
        public int Execute(string sql)
        {
            try
            {
                int rows;

                using (var command = GetCommand(sql))
                {
                    rows = command.ExecuteNonQuery();
                }

                return rows;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (MySqlException ex)
            {
                throw new ExcecaoExecutarSQL(ex.Message);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        /// <summary>
        /// Retorna o valor da primeira coluna da primeira linha do resultado da query passada por parâmetro.
        /// </summary>
        /// <param name="sql">Query</param>
        public string GetValue(string sql)
        {
            return GetValue(GetCommand(sql));
        }

        /// <summary>
        /// Retorna o valor da primeira coluna da primeira linha do resultado do SqlCommand passado por parâmetro.
        /// </summary>
        /// <param name="sqlCommand">SqlCommand</param>
        public string GetValue(MySqlCommand sqlCommand)
        {
            try
            {
                string value;

                using (var command = sqlCommand)
                {
                    value = command.ExecuteScalar().ToString();
                }

                return value;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (MySqlException ex)
            {
                throw new ExcecaoExecutarSQL(ex.Message);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        /// <summary>
        /// Retorna o SqlDataReader baseado no texto da query passada pelo parâmetro sql.
        /// DataReader = Conectado ao banco de dados, a conexão não pode ser fechada.
        /// </summary>
        /// <param name="sql">Query</param>
        /// <returns></returns>
        public MySqlDataReader GetDataReader(string sql)
        {
            try
            {
                MySqlDataReader dataReader;

                using (MySqlCommand command = GetCommand(sql))
                {
                    dataReader = command.ExecuteReader();
                }

                return dataReader;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (MySqlException ex)
            {
                throw new ExcecaoExecutarSQL(ex.Message);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        /// <summary>
        /// Retorna o DataSet baseado no texto da query passada pelo parâmetro sql.
        /// DataSet = Desconectado ao banco de dados, a conexão pode ser fechada.
        /// </summary>
        /// <param name="sql">Query</param>
        public DataSet GetDataSet(string sql)
        {
            try
            {
                var dataSet = new DataSet();

                using (var dataAdapter = new MySqlDataAdapter(GetCommand(sql)))
                {
                    dataAdapter.Fill(dataSet, "0");
                }

                return dataSet;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (MySqlException ex)
            {
                throw new ExcecaoExecutarSQL(ex.Message);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        //Percorrendo um DataSet
        //if (datatable1 !=null)
        //{
        //    foreach (DataRow dr i datatable1.Rows)
        //    {
        //        string text1 = dr["Column1Name Or Number"].ToString();
        //        string text2 = dr["Column1Name Or Number"].ToString();
        //    }
        //}

        /// <summary>
        /// Retorna o DataTable baseado no texto da query passada pelo parâmetro sql.
        /// DataTable = Desconectado ao banco de dados, a conexão pode ser fechada.
        /// </summary>
        /// <param name="sql">Query</param>
        public DataTable GetDataTable(string sql)
        {
            return GetDataTable(GetCommand(sql));
        }

        /// <summary>
        /// Retorna o DataTable baseado no SqlCommand passado pelo parâmetro sqlCommand.
        /// DataTable = Desconectado ao banco de dados, a conexão pode ser fechada.
        /// </summary>
        /// <param name="sqlCommand">SqlCommand</param>
        public DataTable GetDataTable(MySqlCommand sqlCommand)
        {
            try
            {
                var dataTable = new DataTable();

                using (var dataAdapter = new MySqlDataAdapter(sqlCommand))
                {
                    dataAdapter.Fill(dataTable);
                }

                return dataTable;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (MySqlException ex)
            {
                throw new ExcecaoExecutarSQL(ex.Message);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        //Percorrendo um DataTable
        //static void Main()
        //{
        //    DataTable table = GetTable(); // Get the data table.
        //    foreach (DataRow row in table.Rows) // Loop over the rows.
        //    {
        //        Console.WriteLine("--- Row ---"); // Print separator.
        //        foreach (var item in row.ItemArray) // Loop over the items.
        //        {
        //            Console.Write("Item: "); // Print label.
        //            Console.WriteLine(item); // Invokes ToString abstract method.
        //        }
        //    }
        //    Console.Read(); // Pause.
        //}

        /// <summary>
        /// Retorna um DataTable baseado no contexto da Stored Procedure passado pelo parâmetro context.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa IStoredProcedureContext</param>
        public DataTable GetDataTable(IStoredProcedureContext context)
        {
            using (var command = GetCommand(context.NAME))
            {
                command.CommandType = CommandType.StoredProcedure;
                context.AddParameters(command);
                return GetDataTable(command);
            }
        }

        /// <summary>
        /// Retorna o valor da primeira coluna da primeira linha do resultado da Stored Procedure passado pelo parâmetro context.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa IStoredProcedureContext</param>
        public string GetValue(IStoredProcedureContext context)
        {
            using (var command = GetCommand(context.NAME))
            {
                command.CommandType = CommandType.StoredProcedure;
                context.AddParameters(command);
                return GetValue(command);
            }
        }

        /// <summary>
        /// Retorna o valor do resultado da Stored Procedure passado pelo parâmetro context.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa IStoredProcedureContext</param>
        public string GetResult(IStoredProcedureContext context)
        {
            try
            {
                string result;

                using (var command = GetCommand(context.NAME))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    context.AddParameters(command);

                    var returnValue = command.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
                    returnValue.Direction = ParameterDirection.ReturnValue;

                    command.ExecuteNonQuery();

                    result = returnValue.Value.ToString();
                }

                return result;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (MySqlException ex)
            {
                throw new ExcecaoExecutarSQL(ex.Message);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        public string DateMySql(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        public string DateTimeMySql(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }
        
        public DateTime DateMySql(string date)
        {
            return Convert.ToDateTime(date);
        }

    }

}

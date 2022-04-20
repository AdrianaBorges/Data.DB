using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Data.Base
{
    /// <summary>
    /// Classe responsável por carregar e selecionar elementos de interface com base em dados do banco de dados.
    /// </summary>
    public class WindowsForm
    {
        struct Dado
        {
            public string Id { get; set; }
            public string Nome { get; set; }
        }

        /// <summary>
        /// Carrega o ListBox com base no conteúdo do DataTable passado por parâmetro.
        /// </summary>
        /// <param name="listBox">ListBox</param>
        /// <param name="dataTable">DataTable</param>
        public void LoadFromDataTable(ListBox listBox, DataTable dataTable)
        {
            try
            {
                var colecao = new List<Dado>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var dado = new Dado { Id = row.ItemArray[0].ToString(), Nome = row.ItemArray[1].ToString() };
                    colecao.Add(dado);
                }

                listBox.ValueMember = "ID";
                listBox.DisplayMember = "Nome";
                listBox.DataSource = colecao;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro :" + ex.Message);
            }
        }

        /// <summary>
        /// Carrega o ComboBox com base no conteúdo do DataTable passado por parâmetro.
        /// </summary>
        /// <param name="comboBox">ComboBox</param>
        /// <param name="dataTable">DataTable</param>
        public void LoadFromDataTable(ComboBox comboBox, DataTable dataTable)
        {
            try
            {
                var colecao = new List<Dado>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var dado = new Dado { Id = row.ItemArray[0].ToString(), Nome = row.ItemArray[1].ToString() };
                    colecao.Add(dado);
                }

                comboBox.ValueMember = "ID";
                comboBox.DisplayMember = "Nome";
                comboBox.DataSource = colecao;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro :" + ex.Message);
            }
        }

        /// <summary>
        /// Carrega o ComboBox com base no conteúdo do DataTable passado por parâmetro.
        /// </summary>
        /// <param name="ToolStripComboBox">ToolStripComboBox</param>
        /// <param name="dataTable">DataTable</param>
        public void LoadFromDataTable(ToolStripComboBox comboBox, DataTable dataTable)
        {
            try
            {
                var colecao = new List<Dado>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var dado = new Dado { Id = row.ItemArray[0].ToString(), Nome = row.ItemArray[1].ToString() };
                    colecao.Add(dado);
                }
                
                comboBox.ComboBox.ValueMember = "ID";
                comboBox.ComboBox.DisplayMember = "Nome";
                comboBox.ComboBox.DataSource = colecao;

                //comboBox.ComboBox.Items.Add(colecao);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro :" + ex.Message);
            }
        }

        /// <summary>
        /// Carrega o ListView com base no conteúdo do DataTable passado por parâmetro.
        /// </summary>
        /// <param name="listView">ListView</param>
        /// <param name="dataTable">DataTable</param>
        public void LoadFromDataTable(ListView listView, DataTable dataTable)
        {
            try
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    var coluna = 0;
                    var dados = new string[row.ItemArray.Length];

                    foreach (var item in row.ItemArray)
                        dados[coluna++] = item.ToString();

                    listView.Items.Add(new ListViewItem(dados));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro :" + ex.Message);
            }
        }

        /// <summary>
        /// Seleciona um item do ListBox com base no Id passado por parâmetro.
        /// </summary>
        /// <param name="listBox">ListBox</param>
        /// <param name="id">Id do item que será selecionado.</param>
        public void SelectById(ListBox listBox, string id)
        {
            foreach (Dado item in listBox.Items)
            {
                if (item.Id != id) continue;
                listBox.SelectedItem = item;
                return;
            }
            throw new Exception("Item não encontrado.");
        }

        /// <summary>
        /// Seleciona um item do ComboBox com base no Id passado por parâmetro.
        /// </summary>
        /// <param name="comboBox">ComboBox</param>
        /// <param name="id">Id do item que será selecionado.</param>
        public void SelectById(ComboBox comboBox, string id)
        {
            foreach (Dado item in comboBox.Items)
            {
                if (item.Id != id) continue;
                comboBox.SelectedItem = item;
                return;
            }
            throw new Exception("Item não encontrado.");
        }

        public static void RegisterFocusEvents(Control.ControlCollection controls)
        {
            if (controls == null) 
                throw new ArgumentNullException("controls");
            
            foreach (Control control in controls)
            {
                if ((control is TextBox) || (control is RichTextBox) || (control is ComboBox) || (control is MaskedTextBox))
                {
                    control.Enter += new EventHandler(controlFocus_Enter);
                    control.Leave += new EventHandler(controlFocus_Leave);
                }
                RegisterFocusEvents(control.Controls);
            }
        }

        public static void controlFocus_Leave(object sender, EventArgs e)
        {
            (sender as Control).BackColor = Color.White;
        }

        public static void controlFocus_Enter(object sender, EventArgs e)
        {
            (sender as Control).BackColor = Color.LightGreen;
        }
    }
}

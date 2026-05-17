using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PZG_vitamix
{
    public partial class Form1 : Form
    {
        MySqlConnectionStringBuilder server = new MySqlConnectionStringBuilder
        {
            Server = "127.0.0.1",
            UserID = "root",
            Password = "mysql"
        };

        MySqlConnection kapcsolat;

        public Form1()
        {
            InitializeComponent();
            kapcsolat = new MySqlConnection(server.ConnectionString);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Vitamix Készlet";

            KilépésBtn.BackColor = Color.Yellow;

            if (File.Exists("logo.png"))
            {
                pictureBox1.Image = Image.FromFile("logo.png");
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }

            Adatbazis();
            KiszerelesBetoltes();
            VasarlokBetoltese();
        }

        private void Adatbazis()
        {
            try
            {
                kapcsolat.Open();

                MySqlCommand command = kapcsolat.CreateCommand();

                if (File.Exists("forras.sql"))
                {
                    string sql = File.ReadAllText("forras.sql", Encoding.UTF8);

                    command.CommandText = sql;

                    command.ExecuteNonQuery();
                }

                kapcsolat.Close();

                server.Database = "tapkiegeszito"; //biztos ami biztos

                kapcsolat = new MySqlConnection(server.ConnectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba: " + ex.Message);
            }
        }

        private void KiszerelesBetoltes()
        {
            try
            {
                kapcsolat.Open();

                MySqlCommand command = new MySqlCommand(
                    "SELECT DISTINCT kiszereles FROM termek ORDER BY kiszereles DESC",
                    kapcsolat);

                MySqlDataReader reader = command.ExecuteReader();

                comboBox1.Items.Clear();

                while (reader.Read())
                {
                    comboBox1.Items.Add(reader["kiszereles"].ToString());
                }

                reader.Close();
                kapcsolat.Close();

                if (comboBox1.Items.Count > 0)
                {
                    comboBox1.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba: " + ex.Message);

                kapcsolat.Close();
            }
        }

        private void VasarlokBetoltese()
        {
            try
            {
                kapcsolat.Open();

                string sql = "SELECT nev FROM vasarlo ORDER BY nev";

                MySqlCommand command = new MySqlCommand(sql, kapcsolat);

                MySqlDataReader reader = command.ExecuteReader();

                listBox1.Items.Clear();

                while (reader.Read())
                {
                    listBox1.Items.Add(reader["nev"].ToString());
                }

                reader.Close();
                kapcsolat.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba: " + ex.Message);

                kapcsolat.Close();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                kapcsolat.Open();

                MySqlCommand command = new MySqlCommand(
                    "SELECT COUNT(id) FROM termek WHERE kiszereles = @k",
                    kapcsolat);

                command.Parameters.AddWithValue("@k", comboBox1.SelectedItem);

                label1.Text =
                    $"Ebből a kiszerelésből {command.ExecuteScalar()} db van készleten.";

                kapcsolat.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba: " + ex.Message);

                kapcsolat.Close();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                kapcsolat.Open();

                string sql =
                    @"SELECT SUM(termek.egysegar * eladas.mennyiseg) AS osszeg
                      FROM eladas
                      INNER JOIN vasarlo
                      ON eladas.vasarloId = vasarlo.id
                      INNER JOIN termek
                      ON eladas.termekId = termek.id
                      WHERE vasarlo.nev = @nev";

                MySqlCommand command = new MySqlCommand(sql, kapcsolat);

                command.Parameters.AddWithValue(
                    "@nev",
                    listBox1.SelectedItem.ToString());

                object eredmeny = command.ExecuteScalar();

                if (eredmeny == DBNull.Value)
                {
                    MessageBox.Show(
                        "Nincs vásárlási előzmény ezen a néven.");
                }
                else
                {
                    label2.Text = $"{eredmeny} Ft";
                }

                kapcsolat.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba: " + ex.Message);

                kapcsolat.Close();
            }
        }

        private void KilépésBtn_MouseLeave(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;


namespace Fogadas_zaro
{
    class Database
    {
        static public MySqlCommand command;
        static public MySqlConnection connection;

        public Database(string server = "localhost", string user = "root", string password = "", string db = "fogadas_zaro")
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.Server = server;
            builder.UserID = user;
            builder.Password = password;
            builder.Database = db;
            connection = new MySqlConnection(builder.ConnectionString);
            if (Kapcsolatok())
            {
                command = connection.CreateCommand();
            }
        }
        private bool Kapcsolatok()
        {
            try
            {
                connection.Open();
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public int getcsapatnyer(int csapat_id)
        {
            int temp = 0;
            command.Parameters.Clear();
            command.CommandText = "SELECT * FROM `meccs_eredmeny` WHERE `hazai_id` = @id OR `vendeg_id` = @id ORDER BY meccs_id DESC LIMIT 5;";
            command.Parameters.AddWithValue("@id", csapat_id);
            connection.Open();
            using (MySqlDataReader dr = command.ExecuteReader())
            {
                while (dr.Read())
                {
                    int hazai = dr.GetInt32("hazai_id");
                    int vendeg = dr.GetInt32("vendeg_id");
                    int[] eredmeny = Array.ConvertAll(dr.GetString("eredmeny").Split('-'), int.Parse);
                    if (hazai == csapat_id)
                    {
                        if (eredmeny[0] > eredmeny[1])
                        {
                            temp++;
                        }
                    }
                    else if (vendeg == csapat_id)
                    {
                        if (eredmeny[0] < eredmeny[1])
                        {
                            temp++;
                        }
                    }
                }
            }
            connection.Close();
            return temp;
        }
        public string[] csapatnev(int[] csapat)
        {
            string[] csapatnev = new string[2];
            for (int i = 0; i < 2; i++)
            {
                command.Parameters.Clear();
                command.CommandText = "SELECT nemzet_nev FROM `nemzetek`WHERE nemzet_id = @id ";
                command.Parameters.AddWithValue("@id", csapat[i]);
                connection.Open();
                using (MySqlDataReader dr = command.ExecuteReader())
                {
                    dr.Read();
                    string csapatneve = dr.GetString("nemzet_nev");
                    csapatnev[i] = csapatneve;
                }
                connection.Close();
            }            
            return csapatnev;
        }
        public int[] csapatsorsolas(Random rng)
        {   
            int[] adatvissza = new int[2];
            command.Parameters.Clear();
            command.CommandText = "SELECT COUNT(`nemzet_id`) AS 'idcount'FROM `nemzetek`";
            connection.Open();

            using (MySqlDataReader dr = command.ExecuteReader())
            {
                dr.Read();
                int temp = dr.GetInt32("idcount")+1;
                adatvissza[0] = rng.Next(1, temp);
                bool a = true;
                while (a)
                {
                    adatvissza[1] = rng.Next(1, temp);
                    if (adatvissza[0] != adatvissza[1])
                    {
                        a = false;
                    }
                }
                
            }
            connection.Close();
            return  adatvissza;
        }

        public List<Jatekos> gol_lovo(Random rng,int csapatid,int golok_szama)
        {
            command.Parameters.Clear();        
            command.CommandText = "SELECT * FROM jatekosok WHERE nemzet_id = @id";
            command.Parameters.AddWithValue("@id", csapatid);
            connection.Open();

            var jatekosok = new List<Jatekos>();
            using (MySqlDataReader dr = command.ExecuteReader()) 
            {
                while (dr.Read())
                {
                    int nemzet_id = dr.GetInt32("nemzet_id");
                    string nev = dr.GetString("jatekos_nev");
                    string pozicio = dr.GetString("pozicio");
                    jatekosok.Add(new Jatekos(nemzet_id, nev, pozicio));
                }
            } 
            
            //próba sorsolás gól 
            List<Jatekos> golovok = new List<Jatekos>();
            for (int i = 0; i < golok_szama; i++)
            {
                Jatekos goljelolt = null;
                while (goljelolt == null)
                {
                    int valasztottJatekosIndex = rng.Next(jatekosok.Count);
                    Jatekos valasztottJatekos = jatekosok[valasztottJatekosIndex];

                    switch (valasztottJatekos.Pozicio)
                    {
                        case "Kapus":
                            break;
                        case "Védő":
                            if (rng.Next(0,10) == 0)//10% esély védőre
                            {
                                goljelolt = valasztottJatekos;
                                golovok.Add(goljelolt);
                            }
                            break;
                        case "Középpályás":
                            if (rng.Next(0,5) == 0)//20% esély középpályásra
                            {
                                goljelolt = valasztottJatekos;
                                golovok.Add(goljelolt);
                            }
                            break;
                        case "Csatár":
                            if (rng.Next(0,2) == 0)//50% esély csatárra
                            {
                                goljelolt = valasztottJatekos;
                                golovok.Add(goljelolt);
                            }
                            break;
                    }
                }
            }
            connection.Close(); 
            return golovok;         
        }
        

        public void adatkiiratas(string eredmeny, List<Jatekos> gollovo, int golszam, int hazaiid, int vendegid)
        {
            string jatekos = "";
            foreach ( Jatekos j in gollovo)
            {
               jatekos += j.Jatekos_nev + ",";
            }
            command.Parameters.Clear();
            command.CommandText = "INSERT INTO meccs_eredmeny (eredmeny,gol_szerzo,golszam,hazai_id,vendeg_id) VALUES(@eredmeny, @golszerzo, @golszam, @hazai, @vendeg); ";
            command.Parameters.AddWithValue("@eredmeny", eredmeny);
            command.Parameters.AddWithValue("@golszerzo", jatekos);
            command.Parameters.AddWithValue("@golszam", golszam);
            command.Parameters.AddWithValue("@hazai", hazaiid);
            command.Parameters.AddWithValue("@vendeg", vendegid);
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();

        }
        public void adatkiiratasszorzok(double[] szorzo, double dontetlen)
        {
            connection.Open();
           
            command.Parameters.Clear();

            command.CommandText = "UPDATE fogadasi_lehetoseg SET szorzo = @szorzo WHERE fogadasi_szam = 1;"; 
            command.Parameters.AddWithValue("@szorzo", szorzo[0]);
            command.ExecuteNonQuery();
            command.Parameters.Clear();
            command.CommandText = "UPDATE fogadasi_lehetoseg SET szorzo = @szorzo WHERE fogadasi_szam = 2;";
            command.Parameters.AddWithValue("@szorzo", szorzo[1]);
            command.ExecuteNonQuery();
            command.Parameters.Clear();
            command.CommandText = "UPDATE fogadasi_lehetoseg SET szorzo = @szorzo WHERE fogadasi_szam = 3;";
            command.Parameters.AddWithValue("@szorzo", dontetlen);
            command.ExecuteNonQuery();
            command.Parameters.Clear();
            connection.Close();

        }
    }
}

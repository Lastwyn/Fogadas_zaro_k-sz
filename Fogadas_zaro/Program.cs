using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;


namespace Fogadas_zaro
{
    class Program
    {
       

        static int[] sorsolas(double hazairange, double vendegrange,Random rng)
        {
            int[] az = new int[3];
            double random = Convert.ToDouble(rng.Next(-1000, 1001)) / 1000;
            int fix = rng.Next(1, 5);
            if (random > hazairange)
            {
                double szorzo = ((random - hazairange) / (1 - hazairange));
                double hazaipont = Math.Round(fix + szorzo * fix);
                double vendegpont = Math.Floor(fix - szorzo * fix); 
                int golszam = Convert.ToInt32(hazaipont + vendegpont);
                Console.WriteLine($"{golszam} gól volt a meccsen");
                Console.WriteLine($"{hazaipont}-{vendegpont}");
                Console.WriteLine("Hazai nyert");
                az[1] = Convert.ToInt32(hazaipont);
                az[2] = Convert.ToInt32(vendegpont);
                az[0] = 0;
                return az;
            }
            else if (random < vendegrange)
            {
                double szorzo = ((random - vendegrange) / (-1 - vendegrange));
                double vendegpont = Math.Round(fix + szorzo * fix);
                double hazaipont = Math.Floor(fix - szorzo * fix);
                int golszam = Convert.ToInt32(hazaipont + vendegpont);
                Console.WriteLine($"{golszam} gól volt a meccsen");
                Console.WriteLine($"{hazaipont}-{vendegpont}");
                Console.WriteLine("Vendég nyert");
                az[1] = Convert.ToInt32(hazaipont);
                az[2] = Convert.ToInt32(vendegpont);
                az[0] = 1;
                return az;
            }
            else
            {
                int golszam = fix * 2;
                Console.WriteLine($"{golszam} gól volt a meccsen");
                Console.WriteLine($"{fix}-{fix}");
                Console.WriteLine("Döntetlen");
                az[1] = Convert.ToInt32(fix);
                az[2] = Convert.ToInt32(fix);
                az[0] = 2;
                return az;
            }
            
        }
    
        static double[] GenerateMultiplier(double hazainyer, double vendegnyer)
        {
            double[] szorzok = new double[2];
            // Szorzó kiszámítása
            double osszwin = hazainyer+1 + vendegnyer+1;
            double hazaiszorzo = osszwin / (hazainyer+1);          
            double vendegszorzo = osszwin / (vendegnyer+1);
           
            if (hazaiszorzo>vendegszorzo)
            {
                vendegszorzo = vendegszorzo * 1.2;
            }
            if (hazaiszorzo<vendegszorzo)
            {
                hazaiszorzo = hazaiszorzo * 1.2;
            }  
            szorzok[0] = Math.Round(hazaiszorzo, 2); 
            szorzok[1] = Math.Round(vendegszorzo, 2);
            return szorzok;
        }
        // Szorzó generálása a döntetlenre
        static double GenerateDrawMultiplier(double homeTeamMultiplier, double awayTeamMultiplier)
        {
            double drawMultiplier =(homeTeamMultiplier + awayTeamMultiplier) / 1.5;
            return drawMultiplier;
        }
        static void Main(string[] args)
        {

            Random rng = new Random(Guid.NewGuid().GetHashCode());
            Database database = new Database();
            int[] csapatok = database.csapatsorsolas(rng);        
            string[] csapatnev = database.csapatnev(csapatok);
            int hazainyer = database.getcsapatnyer(csapatok[0]);
            int vendegnyer = database.getcsapatnyer(csapatok[1]);
           
            double suly = Convert.ToDouble(vendegnyer - hazainyer) / 10;
            double hazaisuly = 0;
            if (Math.Abs(suly) == suly)
            {
                hazaisuly = suly * 1.5;
            }
            else
            {
                hazaisuly = suly;
            }
            double vendegsuly = 0;
            if (Math.Abs(suly) != suly)
            {
                vendegsuly = suly * 1.5;
            }
            else
            {
                vendegsuly = suly;
            }
            //szorzok
            double[] multiplier = GenerateMultiplier(hazainyer, vendegnyer);
            double drawMultiplier = Math.Round(GenerateDrawMultiplier(multiplier[0], multiplier[1]), 2);
            Console.WriteLine("Hazai: " + multiplier[0]);
            Console.WriteLine("Vendég: " + multiplier[1]);
            Console.WriteLine("Döntetlen: " + drawMultiplier);
            database.adatkiiratasszorzok(multiplier, drawMultiplier);
            //meccs
            double hazairange = 0.2 + hazaisuly;
            double vendegrange = -0.2 + vendegsuly;
            Console.WriteLine(hazairange);
            Console.WriteLine(vendegrange);
            Console.WriteLine(hazainyer);
            Console.WriteLine(vendegnyer);

            Console.WriteLine($"{csapatok[0]} {csapatok[1]}");
            Console.WriteLine($"{csapatnev[0]} {csapatnev[1]}");           
            int[] eredmeny = sorsolas(hazairange,vendegrange,rng);

            List<Jatekos> hazaigollovok = database.gol_lovo(rng, csapatok[0], eredmeny[1]);
            List<Jatekos> vendeggollovok = database.gol_lovo(rng, csapatok[1], eredmeny[2]);
            foreach (Jatekos jatekos in hazaigollovok)
            {
                Console.WriteLine($"{jatekos.Jatekos_nev}---{jatekos.Pozicio}");
            }
            foreach (Jatekos jatekos in vendeggollovok)
            {
                Console.WriteLine($"{jatekos.Jatekos_nev}---{jatekos.Pozicio}");
            }

          //  database.adatkiiratas($"{eredmeny[1]}-{eredmeny[2]}", hazaigollovok.Concat(vendeggollovok).ToList(), eredmeny[1] + eredmeny[2], csapatok[0], csapatok[1]);
         
            
            Console.ReadKey();
           
        }
        

    }
}

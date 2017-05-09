using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Globalization;
using System.Resources;
using System.Windows.Media;

namespace Metro
{
    class KyivMetroData
    {
        public struct station
        {
            public Ellipse ellipse;
            public int line;
        }

        //Dictionary to hold all stations by their id
        public Dictionary<int, station> stations = new Dictionary<int, station>();
        //Dictionary to hold pair of lines and pair of stations that connect them
        public Dictionary<Tuple<int, int>, Tuple<int, int>> transfers = new Dictionary<Tuple<int, int>, Tuple<int, int>>();

        public KyivMetroData()
        {           
            ResourceSet resourceSet = Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true);

            //Reading all stations possitions on a map and connections between them
            station temp;
            try
            {
                foreach (DictionaryEntry entry in resourceSet)
                {
                    if (entry.Value is string && entry.Key.ToString().Substring(0, 2) == "Id")
                    {
                        //each station have its line and in line there are coordinates on a map, lineId and stationId
                        string[] buff = entry.Value.ToString().Split();

                        temp = new station();
                        temp.ellipse = new Ellipse();
                        temp.ellipse.Margin = new Thickness(int.Parse(buff[0]), int.Parse(buff[1]), 0, 0);
                        temp.line = int.Parse(buff[2]);
                        temp.ellipse.Width = 18;
                        temp.ellipse.Height = 18;
                        temp.ellipse.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                        int id = int.Parse(buff[3]);

                        stations.Add(id, temp);
                    }
                    else if (entry.Value is string && entry.Key.ToString().Substring(0, 4) == "Pair")
                    {
                        string[] buff = entry.Value.ToString().Split();

                        Tuple<int, int> lines = new Tuple<int, int>(int.Parse(buff[0]), int.Parse(buff[1]));
                        Tuple<int, int> stationsIds = new Tuple<int, int>(int.Parse(buff[2]), int.Parse(buff[3]));

                        transfers.Add(lines, stationsIds);
                    }
                }
            }
            catch { MessageBox.Show("Something went wrong."); }
        }

        public List<Ellipse> FindPath(Ellipse from, Ellipse to)
        {
            List<Ellipse> res = new List<Ellipse>();

            int startId = stations.First(a => a.Value.ellipse == from).Key;
            int goalId = stations.First(a => a.Value.ellipse == to).Key;

            int curr = startId;
            int tempGoal = goalId;
            bool withTransf = false;
            int transfToId = 0;

            while (curr != goalId)
            {
                withTransf = false;
                if (stations[curr].line != stations[tempGoal].line)
                {
                    //if stations on different lines, program search pair that connect these lines and go throw this 
                    //tranfer to destination station. More difficult search algorithms don't nessesery for Kyiv metropoliten.
                    withTransf = true;
                    int line1 = Math.Min(stations[curr].line, stations[tempGoal].line);
                    int line2 = Math.Max(stations[curr].line, stations[tempGoal].line);

                    Tuple<int, int> lines = new Tuple<int, int>(line1, line2);

                    var transf = transfers[lines];
                    if (stations[transf.Item1].line == stations[curr].line)
                    {
                        tempGoal = transf.Item1;
                        transfToId = transf.Item2;
                    }
                    else
                    {
                        tempGoal = transf.Item2;
                        transfToId = transf.Item1;
                    }
                }

                for (int i = curr; i != tempGoal; i += (curr < tempGoal) ? 1 : -1)
                {
                    res.Add(stations[i].ellipse);
                }
                res.Add(stations[tempGoal].ellipse);

                curr = (withTransf) ? transfToId : tempGoal;
                tempGoal = goalId;
                if (curr == tempGoal && withTransf)
                    res.Add(stations[curr].ellipse);
            }

            return res;
        }

    }
}

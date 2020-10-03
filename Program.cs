using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace JoinVersusSelectWhere
{
    public class City
    {
        public string Id {get;}
        public string Name{get;}
        public string DepartmentCode{get;}

        public City(string rawData)
        {
            var arr = rawData.Split(';');
            Id = arr[0].Replace("COMMUNE_", string.Empty);
            Name = arr[1];
            DepartmentCode = arr[8];
        }
    }

    public class Department
    {
        public string Code {get;}
        public string Name {get;}

        public Department(string rawData)
        {
            var arr = rawData.Split(';');
            Code = arr[8];
            Name = arr[7];
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var cityList = GetCityList();
            var departmentList = GetDepartmentList()
                                .GroupBy(t => t.Code)
                                .Select(t => t.First())                                
                                .ToList();


            var lst = new List<float>();

            for(var idx = 0; idx < 10000; idx++)
            {
                var sw1 = Stopwatch.StartNew();
                
                var selectedList = cityList
                                    .Select(c => new { City = c.Name, Department = departmentList.Where(d => d.Code == c.DepartmentCode).FirstOrDefault()?.Name })
                                    .ToList();

                sw1.Stop();
                var ellapsedTime1 = sw1.ElapsedMilliseconds;


                var sw2 = Stopwatch.StartNew();

                var selectedList2 = cityList.Join(
                                        departmentList,
                                        c => c.DepartmentCode,
                                        d => d.Code,
                                        (key, g) => new { City = key.Name, Department = g.Name }
                                    )
                                    .ToList();

                sw2.Stop();
                var ellapsedTime2 = sw2.ElapsedMilliseconds;

                lst.Add((float)ellapsedTime1/ellapsedTime2);

                Console.WriteLine($"{idx} - {ellapsedTime1} : {ellapsedTime2} | {(float)ellapsedTime1/ellapsedTime2}");
            }

            var avg = lst.Average();
            

            var squarredStd = lst.Select(t => Math.Pow((float)(t-avg), 2)).Aggregate((r, acc) => r + acc);
            var sdt = Math.Sqrt(squarredStd/lst.Count);

            Console.WriteLine($"Average : {avg}");
            Console.WriteLine($"Standard deviation : {sdt}");

            Console.WriteLine("Hello World!");            
        }


        private static IList<City> GetCityList()
        {
            var rows = File.ReadAllLines(@".\Data\liste-des-communes-2019-without-header.csv");
            var cityList = rows.Select(str => new City(str)).ToList();
            return cityList;            
        }

        private static IList<Department> GetDepartmentList()
        {
            var rows = File.ReadAllLines(@".\Data\liste-des-communes-2019-without-header.csv");
            var departmentList = rows.Select(str => new Department(str)).ToList();
            return departmentList;            
        }        

    }
}


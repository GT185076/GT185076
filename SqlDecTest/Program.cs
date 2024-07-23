﻿using Npgsql;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using SQLDecorator;
using DBTables;

namespace SqlDecTest
{
        class Program
        {
            static async Task Main(string[] args)
            {               
                Console.WriteLine("Data Base Object Mapper");
                Console.WriteLine("-----------------------");               
                RunMssql();
                Console.ReadKey();
            }
          
            static async Task RunPostGress()
            {
                var connString = "Host=localhost;Username=postgres;Password=admin1234;Database=postgres";

                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                var cr = new cacheReady();
                var cr2 = new cacheReady();

                var select = new Select(conn).TableAdd(cr, "redis")
                                      .ColumnAdd(cr.PKey)
                                      .ColumnAdd(cr.PValue, "Value")
                                      .ColumnAdd(cr.IsActive)
                                      .ColumnAdd(cr.Height, "Height")
                                      .ColumnAdd(cr.createTS, "Create TS")
                                      .AndNot(cr.PKey.Equal("gadi")
                                                       .And(cr.IsActive)
                                                       .AndNot(cr.PValue.Equal("LEON")))
                                      .And(cr.PValue.Equal("toledano"))
                                      .Or(cr.PValue.Equal(cr.PKey))
                                      .LeftJoinTableAdd(cr2, cr2.PKey.Equal("5"), "selfi");

                var selectString = select.ToString();
                Console.WriteLine(selectString);

                Console.ReadKey();
                Console.WriteLine();

                foreach (var c in select.SelectedFields)
                    Console.Write(c.Caption + "\t\t");
                Console.WriteLine();

                foreach (var c in select.SelectedFields)
                    Console.Write(string.Empty.PadLeft(c.Caption.Length, '-') + "\t\t");
                Console.WriteLine();

                var runner = new PostGressSelectRunner();

                foreach (var record in runner.Run(select, conn))
                {
                    foreach (var f in record.Columns)
                        Console.Write($"{f}\t\t");
                    Console.WriteLine();
                }

                Console.WriteLine();
                Console.Write($"{select.Result.Count} Rows Selected.");
            }
            static void RunMssql()
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                builder.DataSource = "WILGT185076-R4B";
                builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated;
                builder.InitialCatalog = "STS_Grocery";
                builder.TrustServerCertificate = true;

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    Console.WriteLine("\nQuery data example:");
                    Console.WriteLine("=========================================\n");

                    connection.Open();

                    var product = new Product();
                    var price   = new Price();
                    var vPrice  = new IntegerColumn("Cents", "Max(Prices.Price)");

                    var select = new Select(connection)
                                   .TableAdd(product, "Products")
                                   .TableAdd(price, "Prices")
                                   .ColumnAdd(product.Product_Id)
                                   .ColumnAdd(vPrice)
                                   .Where(price.Product_Id.Equal(product.Product_Id))
                                   .And(price.EffectiveDate
                                   .GreaterThan(DateTime.Now - new TimeSpan(365, 0, 0, 0, 0)))
                                   .GroupByAdd(product.Product_Id)
                                   .OrderByAdd(OrderBy.Asc, vPrice);
                              

                    Console.WriteLine(select.ToString());

                    printCaptions(select);

                    foreach (var record in select.Run())
                    {
                        foreach (var f in record.Columns) Console.Write($"{f}\t\t");
                        Console.WriteLine();
                    }
                    Console.WriteLine($"\n{select.Result.Count} Rows Selected.\n");
                    Console.ReadKey();


                var select2 = new Select(connection)
                                   .Distict()
                                   .Top(20)
                                   .TableAdd(product, "Products", ColumnsSelection.All)
                                   .TableAdd(price, "Prices")                                   
                                   .Where(price.Product_Id.Equal(product.Product_Id))
                                   .And(price.EffectiveDate
                                   .GreaterThan(DateTime.Now - new TimeSpan(365, 0, 0, 0, 0)));                                                                     

                Console.WriteLine($"Print : {product.Caption} \n");

                Console.WriteLine(select2.ToString());

                printCaptions(select2);

                foreach (var record in select2.Run())
                {
                    foreach (var f in record.Columns) Console.Write($"{f}\t\t");
                    Console.WriteLine();
                }

                foreach (var record in select2.Result.Export<Product>())
                    {
                        Console.WriteLine("{0} {1} {2} {3} {4}",
                            record.Product_Id, record.UnitOfMeasure, record.Weight, record.IsNonMerchandise, record.LastUpdated);
                        Console.WriteLine();
                    }
                    Console.ReadKey();

                }
            }
            private static void printCaptions(Select select)
            {
                Console.WriteLine();
                foreach (var c in select.SelectedFields)
                    Console.Write(c.Caption + "\t\t");
                Console.WriteLine();
                foreach (var c in select.SelectedFields)
                    Console.Write(string.Empty.PadLeft(c.Caption.Length, '-') + "\t\t");
                Console.WriteLine();
            }
        }
    }


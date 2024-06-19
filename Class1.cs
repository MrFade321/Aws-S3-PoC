using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace MeshStream
{
    public class Stream
    {
        public List<String> FlightTiles { get; set; }
        public List<String> GroundTiles { get; set; }
        public List<String> DungTiles { get; set; }

        public bool ListesUpdated { get; set; }

        public string ClientVersion { get; set; }

        internal AmazonS3Client client { get; set; }
        internal string bucketName = "wotlkmeshes";


        public Stream(string clientVersion)
        {
            client = new // Your keys and AWS info would go here - make sure to have a key with LIMITED acess or risk giving acesss to random people...
            FlightTiles = new List<String>();
            GroundTiles = new List<String>();
            DungTiles = new List<String>();


            Console.WriteLine($"[MeshStreamer] => {clientVersion}");


            ClientVersion = clientVersion;

            if (clientVersion == "Dragonflight")
            {
                bucketName = "dragonflightmeshes";
            }

            if (clientVersion == "Wrath of the Lich King")
            {
                bucketName = "wotlkmeshes";
            }

            if (clientVersion == "Vanilla")
            {
                bucketName = "vanillameshes";
            }

            if (clientVersion == "Cataclysm")
            {
                bucketName = "catameshes";
            }


        }



        public async Task<bool> StreamUpdateList()
        {

            try
            {
              // return await ReadTileJson(); This will dump the tiles from our S3 bucket.


                ListObjectsRequest listrequest = new ListObjectsRequest()
                {
                    BucketName = bucketName,
                };


                ListObjectsResponse listResponse;
                //listResponse = await client.ListObjectsAsync(listrequest);

                int ObjCount = 0;

                List<string> GroundTilesT = new List<string>();
                List<string> FlightTilesT = new List<string>();
                List<string> DungTilesT = new List<string>();

                do
                {
                    listResponse = await client.ListObjectsAsync(listrequest);
                    //listResponse = client.ListObjects(listrequest);

                    Console.WriteLine("Dumping existing object list [!]");
                    foreach (var Object in listResponse.S3Objects)
                    {
                        //         Console.WriteLine($"[*]- {Object.Key} : {Object.Size} : {Object.LastModified} ");

                        if (Object.Key.Contains("Flight"))
                        {
                            string temp = Object.Key;
                            int pos = temp.LastIndexOf("/");
                            string Final = temp.Remove(0, pos + 1);
                            //   Console.WriteLine($"{Final}");
                            FlightTilesT.Add(Final);
                        }
                        else if (Object.Key.Contains("Dung"))
                        {
                            string temp = Object.Key;
                            int pos = temp.LastIndexOf("/");
                            string Final = temp.Remove(0, pos + 1);
                            DungTilesT.Add(Final);
                        }
                        else
                        {
                            string temp = Object.Key;
                            int pos = temp.LastIndexOf("/");
                            string Final = temp.Remove(0, pos + 1);
                            GroundTilesT.Add(Final);
                        }

                    }

                    ObjCount = ObjCount + listResponse.S3Objects.Count;
                    listrequest.Marker = listResponse.NextMarker;  // This will increment our "page" or object count IE the next 1k objects.
                } while (listResponse.IsTruncated);

                Console.WriteLine($"[*] -> Objects returned {ObjCount}");
                Console.WriteLine($"[*] -> Dung Tiles : {DungTilesT.Count}");
                Console.WriteLine($"[*] -> Flight Tiles : {FlightTilesT.Count}");
                Console.WriteLine($"[*] -> Ground Tiles : {GroundTilesT.Count}");


                FlightTiles = FlightTilesT;
                GroundTiles = GroundTilesT;
                DungTiles = DungTilesT;
                ListesUpdated = true;

                // Create an object to hold all lists
                var tiles = new
                {
                    FlightTiles,
                    GroundTiles,
                    DungTiles
                };

                // Convert to JSON
                string json = JsonConvert.SerializeObject(tiles, Formatting.Indented);

                // Write JSON to a text file
                File.WriteAllText("tiles.json", json);



                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
                Console.WriteLine($"Failed to update existing tile list!? please report this to Mr.Fade!");
                return false;
            }
        }


        // Define a model class to deserialize your JSON content
        public class TilesModel
        {
            public List<string> FlightTiles { get; set; }
            public List<string> GroundTiles { get; set; }
            public List<string> DungTiles { get; set; }
        }


        public async Task<bool> ReadTileJson()
        {
            try
            {
                string objectName = $"tiles.json";


                var getRequest = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectName,
                };


                using (GetObjectResponse response = await client.GetObjectAsync(getRequest))
                {
                    System.IO.Stream responseStream = response.ResponseStream;
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        string jsonContent = await reader.ReadToEndAsync();

                        // Deserialize JSON content into your object
                        var tiles = JsonConvert.DeserializeObject<TilesModel>(jsonContent);

                        // Now you can access your tiles from the tiles object
                        // For example:
                        List<string> FlightTilesT = tiles.FlightTiles;
                        List<string> GroundTilesT = tiles.GroundTiles;
                        List<string> DungTilesT = tiles.DungTiles;


                        //Console.WriteLine($"[*] -> Objects returned {ObjCount}");
                        Console.WriteLine($"[*] -> Dung Tiles : {DungTilesT.Count}");
                        Console.WriteLine($"[*] -> Flight Tiles : {FlightTilesT.Count}");
                        Console.WriteLine($"[*] -> Ground Tiles : {GroundTilesT.Count}");


                        FlightTiles = FlightTilesT;
                        GroundTiles = GroundTilesT;
                        DungTiles = DungTilesT;
                        ListesUpdated = true;

                        // Do something with the tiles...
                    }
                }



                return true;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error saving grabbing tile Json -> : {ex.Message}");
                return false;
            }
        }



    public async Task<bool> StreamDung(int MapID)
        {
            try
            {
                string objectName = $"Dung/{MapID}.dmesh";
                string filePath = Environment.CurrentDirectory + "/Meshes/";


                if (ClientVersion == "Dragonflight")
                {
                    filePath = filePath + "/Retail/";
                }
                else if (ClientVersion == "Vanilla")
                {
                    filePath = filePath + "/Vanilla/";
                }
                else if (ClientVersion == "Cataclysm")
                {
                    filePath = filePath + "/Cata/";
                }




                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectName,
                };


                GetObjectResponse response = await client.GetObjectAsync(request);
                try
                {
                    // Save object to local file
                    await response.WriteResponseStreamToFileAsync($"{filePath}\\Dung\\{MapID}\\{MapID}.dmesh", true, CancellationToken.None);
                    //        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
                    Console.WriteLine("File seems to have pulled down correctly!");
                    return true;
                }
                catch (AmazonS3Exception ex)
                {
                    Console.WriteLine($"Error saving {objectName}: {ex.Message}");
                    return false;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
                return false;
            }
        }


        public async Task<bool> StreamTile(int MapID, int X, int Y, bool Flight = false)
        {
            try
            {
                string objectName = $"{MapID}/{MapID}_{X}_{Y}.tile";

                if (Flight)
                    objectName = $"Flight/{MapID}/{MapID}_{X}_{Y}.tile";


                string filePath = Environment.CurrentDirectory + "/Meshes/";



                if (ClientVersion == "Dragonflight")
                {
                    filePath = filePath + "/Retail/";
                }
                else if (ClientVersion == "Vanilla")
                {
                    filePath = filePath + "/Vanilla/";
                }
                else if (ClientVersion == "Cataclysm")
                {
                    filePath = filePath + "/Cata/";
                }

           


                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectName,
                };


                GetObjectResponse response = await client.GetObjectAsync(request);
                try
                {
                    // Save object to local file
                    await response.WriteResponseStreamToFileAsync($"{filePath}\\{objectName}", true, CancellationToken.None);
                    //        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
                    Console.WriteLine("File seems to have pulled down correctly!");
                    return true;
                }
                catch (AmazonS3Exception ex)
                {
                    Console.WriteLine($"Error saving {objectName}: {ex.Message}");
                    return false;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
                return false;
            }
        }





    }
    }

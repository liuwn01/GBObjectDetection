using Azure;
using Azure.Core;
using Azure.Identity;
using ConsoleApp1.Labelme.Entities;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1.Labelme
{
    public class Labelme_Main
    {
        private bool _isUserPPEService = true;

        private string _customVisionEndpoint = "https://southcentralus.api.cognitive.microsoft.com/";
        private string _projectName = "LabelMe";//LabelMeDev02
        private bool _isRebuildProject = false;
        private Project _projObj = null;
        private string _classificationType = "Multilabel";
        private string _publishedModelName = "labelme";

        private List<string> _annotations
        {
            get
            {
                return Directory.GetFiles(AnnotationsDirectory, "*.json", SearchOption.AllDirectories).ToList();
            }
        }

        //tag
        Dictionary<string, Tag> tagDict = new Dictionary<string, Tag>();

        private string CurrentDirectory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        private string AnnotationsDirectory
        {
            get
            {
                return Path.Combine(CurrentDirectory, "..", "..", "..", "Datas", "annotations");
            }
        }

        private string TestFilesDirectory
        {
            get
            {
                return Path.Combine(CurrentDirectory, "..", "..", "..", "Datas", "Testing");
            }
        }

        public void run()
        {
            CustomVisionTrainingClient trainingApi = null;
            CustomVisionPredictionClient predictionApi = null;

            if (_isUserPPEService)
            {
                _projectName = $"{_projectName}PPE";
                var tokenCredential = new DefaultAzureCredential();
                var mercuryResourceUri = "https://cognitiveservices.azure.com";
                var tokenRequestContext = new TokenRequestContext(new[] { $"{mercuryResourceUri}/.default" });
                var token = tokenCredential.GetToken(tokenRequestContext).Token;

                trainingApi = new CustomVisionTrainingClient(new Microsoft.Rest.TokenCredentials(token))
                {
                    Endpoint = "https://customvisiongbdocs.cognitiveservices.azure.com/",
                };

                predictionApi = new CustomVisionPredictionClient(new Microsoft.Rest.TokenCredentials(token))
                {
                    Endpoint = "https://customvisiongbdocs-prediction.cognitiveservices.azure.com/"
                };
            }
            else
            {
                trainingApi = new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials("key"))
                {
                    Endpoint = _customVisionEndpoint,
                };

                predictionApi = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials("key"))
                {
                    Endpoint = _customVisionEndpoint
                };

            }

            

            Console.WriteLine("CustomVisionTrainingClient Instance Created");

            //var domains = trainingApi.GetDomains();
            //foreach (var item in domains)
            //{
            //    //2e37d7fb-3a54-486a-b4d6-cfc369af0018; General [A2]; Classification;
            //    //a8e3c40f-fb4a-466f-832a-5e457ae4a344; General [A1]; Classification;
            //    //ee85a74c-405e-4adc-bb47-ffa8ca0c9f31; General; Classification;
            //    //c151d5b5-dd07-472a-acc8-15d29dea8518; Food; Classification;
            //    //ca455789-012d-4b50-9fec-5bb63841c793; Landmarks; Classification;
            //    //b30a91ae-e3c1-4f73-a81e-c270bff27c39; Retail; Classification;
            //    //45badf75-3591-4f26-a705-45678d3e9f5f; Adult; Classification;
            //    //a1db07ca-a19a-4830-bae8-e004a42dc863; General (compact) [S1]; Classification;
            //    //0732100f-1a38-4e49-a514-c9b44c697ab5; General (compact); Classification;
            //    //8882951b-82cd-4c32-970b-d5f8cb8bf6d7; Food (compact); Classification;
            //    //b5cfd229-2ac7-4b2b-8d0a-2b0661344894; Landmarks (compact); Classification;
            //    //6b4faeda-8396-481b-9f8b-177b9fa3097f; Retail (compact); Classification;
            //    //9c616dff-2e7d-ea11-af59-1866da359ce6; General [A1]; ObjectDetection;
            //    //da2e3a8a-40a5-4171-82f4-58522f70fbc1; General; ObjectDetection;
            //    //1d8ffafe-ec40-4fb2-8f90-72b3b6cecea4; Logo; ObjectDetection;
            //    //3780a898-81c3-4516-81ae-3a139614e1f3; Products on Shelves; ObjectDetection;
            //    //7ec2ac80-887b-48a6-8df9-8b1357765430; General (compact) [S1]; ObjectDetection;
            //    //a27d5ca5-bb19-49d8-a70a-fec086c47f5b; General (compact); ObjectDetection; //Export Capabilities
            //    Console.WriteLine($"{item.Id}; {item.Name}; {item.Type}; ");
            //}
            //var objDetectionDomain = domains.FirstOrDefault(d => d.Type == "ObjectDetection");

            string domainId = "1d8ffafe-ec40-4fb2-8f90-72b3b6cecea4";//
            _projObj = trainingApi.GetProjects().Where(p => string.Equals(p.Name, _projectName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (_projObj == null)
            {
                //classificationType=Multiclass&description=test&domainId=a27d5ca5-bb19-49d8-a70a-fec086c47f5b&name=test&useNegativeSet=true
                _projObj = trainingApi.CreateProject(_projectName, "data from labelme", new Guid(domainId), _classificationType);
            }
            else if (_isRebuildProject)
            {
                if (_isUserPPEService)
                {
                    try
                    {
                        trainingApi.DeleteProject(_projObj.Id);
                    }
                    catch (Exception e)
                    {
                        Log_Error(e.ToString());

                        var targetIteration = trainingApi.GetIterations(_projObj.Id).Where(s => s.Status == "Completed").OrderByDescending(s => s.Created).First();
                        trainingApi.UnpublishIteration(_projObj.Id, targetIteration.Id);
                        trainingApi.DeleteProject(_projObj.Id);
                        
                    }
                    _projObj = trainingApi.CreateProject(_projectName, "data from labelme", new Guid(domainId), _classificationType);
                }

            }


            ////Update project properties for training different models
            //1d8ffafe-ec40-4fb2-8f90-72b3b6cecea4; Logo
            //7ec2ac80-887b-48a6-8df9-8b1357765430; General (compact) [S1]
            //9c616dff-2e7d-ea11-af59-1866da359ce6; General [A1]
            //da2e3a8a-40a5-4171-82f4-58522f70fbc1; General
            //3780a898-81c3-4516-81ae-3a139614e1f3; Products on Shelves
            //a27d5ca5-bb19-49d8-a70a-fec086c47f5b; General (compact)
            _projObj.Settings.DomainId = new Guid("da2e3a8a-40a5-4171-82f4-58522f70fbc1");
            _projObj = trainingApi.UpdateProject(_projObj.Id, _projObj);



            //RebuildProject(trainingApi);
            TrainProject(trainingApi);
            ////PublishIteration(trainingApi);
            TestIteration(predictionApi, trainingApi);

            Log_Green($"Done");
            Console.ReadLine();
        }

        public void predict()
        {
            CustomVisionTrainingClient trainingApi = null;
            CustomVisionPredictionClient predictionApi = null;

            _projectName = $"{_projectName}PPE";
            var tokenCredential = new DefaultAzureCredential();
            var mercuryResourceUri = "https://cognitiveservices.azure.com";
            var tokenRequestContext = new TokenRequestContext(new[] { $"{mercuryResourceUri}/.default" });
            var token = tokenCredential.GetToken(tokenRequestContext).Token;

            trainingApi = new CustomVisionTrainingClient(new Microsoft.Rest.TokenCredentials(token))
            {
                Endpoint = "https://customvisiongbdocs.cognitiveservices.azure.com/",
            };

            predictionApi = new CustomVisionPredictionClient(new Microsoft.Rest.TokenCredentials(token))
            {
                Endpoint = "https://customvisiongbdocs-prediction.cognitiveservices.azure.com/"
            };

            Console.WriteLine("CustomVisionTrainingClient Instance Created");
            _projObj = trainingApi.GetProjects().Where(p => string.Equals(p.Name, _projectName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            TestIteration(predictionApi, trainingApi);

            Log_Green($"Done");
            Console.ReadLine();
        }


        private void TestIteration(CustomVisionPredictionClient predictionApi, CustomVisionTrainingClient trainingApi)
        {
            double probabilityLimit = 0.4;
            foreach (string imageFile in Directory.GetFiles(TestFilesDirectory, "*", SearchOption.AllDirectories))
            {
                if (imageFile.Contains("Detected")) continue;
                
                //try
                //{
                //    using (var stream = File.OpenRead(imageFile))
                //    {
                //        var result = predictionApi.DetectImage(_projObj.Id, _publishedModelName, stream);
                //        string json = JsonConvert.SerializeObject(result.Predictions, Formatting.Indented);

                //        // write result to json file
                //        string filePath = Path.Combine(TestFilesDirectory, "Detected", "01.CustomVision", $"{Path.GetFileName(imageFile)}.json");
                //        string outputFilePath = Path.Combine(TestFilesDirectory, "Detected", "01.CustomVision", $"{Path.GetFileName(imageFile)}");
                //        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                //        File.WriteAllText(filePath, json);

                //        using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(imageFile))
                //        {
                //            int image_width = bitmap.Width;
                //            int image_height = bitmap.Height;
                //            Console.WriteLine($"{Path.GetFileName(imageFile)}: Width: {image_width}, Height: {image_height}");

                //            // Loop over each prediction and write out the results
                //            foreach (var c in result.Predictions)
                //            {
                //                if (c.Probability <= probabilityLimit) continue;
                //                double left = c.BoundingBox.Left * image_width;
                //                double top = c.BoundingBox.Top * image_height;
                //                double width = c.BoundingBox.Width * image_width;
                //                double height = c.BoundingBox.Height * image_height;

                //                Console.WriteLine($"\t{c.TagName}: {c.Probability:P1} [ {c.BoundingBox.Left}, {c.BoundingBox.Top}, {c.BoundingBox.Width}, {c.BoundingBox.Height} ]");
                //                Console.WriteLine($"\t{c.TagName}: {c.Probability:P1} [ {left}, {top}, {width}, {height} ]");

                //                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                //                {
                //                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle((int)left, (int)top, (int)width, (int)height);

                //                    using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red, 1))
                //                    {
                //                        graphics.DrawRectangle(pen, rect);
                //                    }
                //                }
                //            }

                //            bitmap.Save(outputFilePath, ImageFormat.Jpeg);
                //        }
                //    }
                //}
                //catch (Exception e)
                {
                    using (var stream = File.OpenRead(imageFile))
                    {
                        var targetIteration = trainingApi.GetIterations(_projObj.Id).Where(s => s.Status == "Completed").OrderByDescending(s => s.Created).First();

                        var result = trainingApi.QuickTestImage(_projObj.Id, stream, targetIteration.Id, false);
                        string json = JsonConvert.SerializeObject(result.Predictions, Formatting.Indented);

                        // write result to json file
                        string filePath = Path.Combine(TestFilesDirectory, "Detected", "01.CustomVision", $"{Path.GetFileName(imageFile)}.json");
                        string outputFilePath = Path.Combine(TestFilesDirectory, "Detected", "01.CustomVision", $"{Path.GetFileName(imageFile)}");
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        File.WriteAllText(filePath, json);

                        using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(imageFile))
                        {
                            int image_width = bitmap.Width;
                            int image_height = bitmap.Height;
                            Console.WriteLine($"{Path.GetFileName(imageFile)}: Width: {image_width}, Height: {image_height}");

                            // Loop over each prediction and write out the results
                            foreach (var c in result.Predictions)
                            {
                                if (c.Probability <= probabilityLimit) continue;
                                double left = c.BoundingBox.Left * image_width;
                                double top = c.BoundingBox.Top * image_height;
                                double width = c.BoundingBox.Width * image_width;
                                double height = c.BoundingBox.Height * image_height;

                                Console.WriteLine($"\t{c.TagName}: {c.Probability:P1} [ {c.BoundingBox.Left}, {c.BoundingBox.Top}, {c.BoundingBox.Width}, {c.BoundingBox.Height} ]");
                                Console.WriteLine($"\t{c.TagName}: {c.Probability:P1} [ {left}, {top}, {width}, {height} ]");

                                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                                {
                                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle((int)left, (int)top, (int)width, (int)height);

                                    using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red, 1))
                                    {
                                        graphics.DrawRectangle(pen, rect);
                                    }
                                }
                            }
                            bitmap.Save(outputFilePath, ImageFormat.Jpeg);
                        }

                    }
                        
                }
                
            }

        }

        private void PublishIteration(CustomVisionTrainingClient trainingApi)
        {
            foreach (var iteration in trainingApi.GetIterations(_projObj.Id))
            {
                Log_Green($"Project '{_projObj.Name}' iteration: {iteration.ProjectId}, {iteration.Id}, {iteration.Name}, {iteration.DomainId}, {iteration.OriginalPublishResourceId}");
            }

            // The iteration is now trained. Publish it to the prediction end point.
            var targetIteration = trainingApi.GetIterations(_projObj.Id).Where(s => s.Status == "Completed").OrderByDescending(s=> s.Created).First();
            string predictionResourceId = "/subscriptions/316b3b07-d051-49f2-9fba-2383bc2e399e/resourceGroups/res-group-1/providers/Microsoft.CognitiveServices/accounts/res-test"; //Resource ID from azure portal
            if (_isUserPPEService)
            {
                predictionResourceId = "/subscriptions/75e2804f-801a-4a5e-9985-c3246b4e1a04/resourceGroups/TestFeature/providers/Microsoft.CognitiveServices/accounts/CustomVisionGBDocs-Prediction";
            }

            try
            {
                trainingApi.UnpublishIteration(_projObj.Id, targetIteration.Id);
            }
            catch (Exception e)
            {
                
                Log_Error(e.ToString());
            }

            
            try
            {
                trainingApi.PublishIterationWithHttpMessages(_projObj.Id, targetIteration.Id, _publishedModelName, predictionResourceId, true);
                var result = trainingApi.PublishIteration(_projObj.Id, targetIteration.Id, _publishedModelName, predictionResourceId, true);
            }
            catch (Exception e)
            {
                Log_Error(e.ToString());
            }

            Console.WriteLine("Done!\n");
        }

        private void RebuildProject(CustomVisionTrainingClient trainingApi)
        {
            AddTags(trainingApi, _annotations);
            if(_isRebuildProject)
            {
                trainingApi.DeleteImages(_projObj.Id, null, true, true); 
                Thread.Sleep(8000);
            }

            foreach (var file in _annotations)
            {
                LabelmeJson annotation = JsonConvert.DeserializeObject<LabelmeJson>(File.ReadAllText(file));
                string imagePath = Path.Combine(AnnotationsDirectory, annotation.imagePath);

                List<Guid> tagIds = new List<Guid>();
                List<Region> regions = new List<Region>();
                double image_width = annotation.imageWidth;
                double image_height = annotation.imageHeight;
                foreach (var shapeItem in annotation.shapes)
                {
                    tagIds.Add(tagDict[shapeItem.label].Id);

                    double left = Math.Floor(shapeItem.points[0][0]) / image_width;
                    double top = Math.Floor(shapeItem.points[0][1]) / image_height;
                    double width = Math.Ceiling(shapeItem.points[1][0] - shapeItem.points[0][0]) / image_width;
                    double height = Math.Ceiling(shapeItem.points[1][1] - shapeItem.points[0][1]) / image_height;
                    var region = new Region(tagDict[shapeItem.label].Id, left, top, width, height);
                    regions.Add(region);
                }

                ImageCreateSummary summary = trainingApi.CreateImagesFromFiles(_projObj.Id, new ImageFileCreateBatch(new List<ImageFileCreateEntry>() { new ImageFileCreateEntry(imagePath, File.ReadAllBytes(imagePath), tagIds, regions) }));
                if (summary.IsBatchSuccessful)
                {
                    Log_Green($"Create image '{Path.GetFileName(imagePath)}' successfully. ");
                }
                else
                {
                    Log_Error($"Failed to create image '{Path.GetFileName(imagePath)}'. error: {summary.Images.FirstOrDefault().Status.ToString()}");
                }
            }
        }

        private void TrainProject(CustomVisionTrainingClient trainingApi)
        {

            // Now there are images with tags start training the project
            Console.WriteLine("\tTraining");

            //Iteration iteration = trainingApi.TrainProject(_projObj.Id);
            Iteration iteration = trainingApi.TrainProject(_projObj.Id, "Advanced", 12);
            
            Log_Green($"iteration: {iteration.ProjectId}, {iteration.Id}, {iteration.Name}");
            //iteration: f672422b-b360-4370-a911-1b5ae44e4953, 60eaddcc-da47-4ebd-ab78-ec67df81918c, Iteration 1

            // The returned iteration will be in progress, and can be queried periodically to see when it has completed
            while (iteration.Status == "Training")
            {
                Thread.Sleep(60000);

                // Re-query the iteration to get its updated status
                iteration = trainingApi.GetIteration(_projObj.Id, iteration.Id);
                Log_Green($"iteration: {iteration.ProjectId}, {iteration.Id}, {iteration.Name}. {iteration.Status}, {iteration.DomainId}");
            }
        }

        private void AddTags(CustomVisionTrainingClient trainingApi, List<string> annotations)
        {
            var tags = trainingApi.GetTags(_projObj.Id);
            foreach (string file in annotations)
            {
                LabelmeJson root = JsonConvert.DeserializeObject<LabelmeJson>(File.ReadAllText(file));
                foreach (var item in root.shapes)
                {
                    if (!tagDict.TryGetValue(item.label, out _))
                    {
                        Tag tag = tags.Where(s => string.Equals(s.Name, item.label, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (tag != null)
                        {
                            tagDict.Add(item.label, tag);
                        }
                        else
                        {
                            tagDict.Add(item.label, trainingApi.CreateTag(_projObj.Id, item.label));
                            Log_Green($"Create tag '{item.label}' on project '{_projObj.Id}'.");
                        }
                        
                    }
                }
            }
        }

        private void Log_Info(string message)
        {
            Console.ResetColor();
            Console.WriteLine(message);
        }

        private void Log_Error(string message)
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
        }

        private void Log_Green(string message)
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
        }
    }
}

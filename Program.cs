using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using porter;
using Accord.Controls;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Statistics;

namespace Classification
{
    class Program
    {
        public struct wordCount
        {
            public int count;
            public int DF;
        }
        public struct tfidfCount
        {
            public double tfidf;
            public double tf;
            public double idf;
            public string word;
        }
        public struct dicWord
        {
            public string word;
            public double tfidf;
        }
        public struct testResult
        {
            public string docName;
            public int oriClass;
            public int resultClass;
        }
        public struct testClassResult
        {
            public int count;
            public int correct;
        }
        public static Hashtable columnCountTable = new Hashtable();
        public static Hashtable wordCountTable = new Hashtable();
        public static Hashtable stopWordTable = new Hashtable();
        public static List<Hashtable> docFeatureTable = new List<Hashtable>();
        public static List<string> fileList = new List<string>();
        public static List<Hashtable> classWordTable = new List<Hashtable>();
        public static Hashtable classIDFTable = new Hashtable();
        
        public static Hashtable TFIDF()
        {
            Hashtable idfTable = new Hashtable();
            StreamWriter file = new StreamWriter(@"D:\work\KPMG\learning\project1\word_1.txt");
            for(int i=0;i<docFeatureTable.Count;i++)
            {
                List<string> wordList = docFeatureTable[i].Keys.Cast<string>().ToList();
                file.WriteLine("doc: " + fileList[i]);
                List<tfidfCount> tempList = new List<tfidfCount>();
                int sumWordCount = 0;
                foreach (string word in wordList)
                {
                    sumWordCount += (int)docFeatureTable[i][word];
                }
                foreach (string word in wordList)
                {
                    double TF = (double)(System.Convert.ToDouble((int)docFeatureTable[i][word]) / sumWordCount);
                    double IDF = (double)classIDFTable[word];
                    idfTable[word] = IDF;
                    docFeatureTable[i][word] = TF * IDF;
                    tfidfCount temp;
                    temp.tf = TF;
                    temp.idf = IDF;
                    temp.tfidf = TF * IDF;
                    temp.word = word;
                    tempList.Add(temp);
                }
                tempList.Sort((a,b)=>b.tfidf.CompareTo(a.tfidf));
                foreach (tfidfCount temp in tempList)
                {
                    file.WriteLine("\t" + temp.word + ": " + temp.tfidf + "/" + temp.tf + "/" + temp.idf);
                }
            }
            return idfTable;
        }
        /*public static Hashtable TFIDF(Hashtable classFeatureTable)
        {
            Hashtable idfTable = new Hashtable();
            StreamWriter file = new StreamWriter(@"D:\work\KPMG\learning\project1\word_1.txt");
            for (int i = 0; i < docFeatureTable.Count; i++)
            {
                List<string> wordList = docFeatureTable[i].Keys.Cast<string>().ToList();
                file.WriteLine("doc: " + fileList[i]);
                List<tfidfCount> tempList = new List<tfidfCount>();
                int sumWordCount = 0;
                foreach (string word in wordList)
                {
                    sumWordCount += (int)docFeatureTable[i][word];
                }
                foreach (string word in wordList)
                {
                    double TF = (double)(System.Convert.ToDouble((int)docFeatureTable[i][word]) / sumWordCount);
                    double IDF = Math.Log((double)docFeatureTable.Count / (double)classIDFTable[word]);
                    idfTable[word] = IDF;
                    docFeatureTable[i][word] = TF * IDF;
                    tfidfCount temp;
                    temp.tf = TF;
                    temp.idf = IDF;
                    temp.tfidf = TF * IDF;
                    temp.word = word;
                    tempList.Add(temp);
                }
                tempList.Sort((a, b) => b.tfidf.CompareTo(a.tfidf));
                foreach (tfidfCount temp in tempList)
                {
                    file.WriteLine("\t" + temp.word + ": " + temp.tfidf + "/" + temp.tf + "/" + temp.idf);
                }
            }
            return idfTable;
        }*/
        public static string[] gen_dic(int size)
        {
            string[] result = new string[10000];
            int dicCount = 0;
            List<List<dicWord>> classWordList = new List<List<dicWord>>();
            for (int i = 0; i < classWordTable.Count(); i++)
            {
                List<dicWord> tempList = new List<dicWord>();
                foreach (string word in classWordTable[i].Keys)
                {
                    dicWord temp;
                    temp.word = word;
                    temp.tfidf = (double)classWordTable[i][word];
                    tempList.Add(temp);
                }
                tempList.Sort((a, b) => b.tfidf.CompareTo(a.tfidf));
                classWordList.Add(tempList);
            }
            for (int i = 0; i < 10000; i++)
            {
                for (int j = 0; j < 20; j++ )
                {
                    if (dicCount < 10000)
                    {
                        if (Array.IndexOf(result, classWordList[j][i].word) == -1)
                        {
                            result[dicCount] = classWordList[j][i].word;
                            dicCount++;
                        }
                    }
                }
            }
            return result;
        }
        public static void gen_test(string sourcePath, string targetPath, int part, int division)
        {
            string baseDir = targetPath + "\\" + part;
            if (!System.IO.Directory.Exists(baseDir))
            {
                System.IO.Directory.CreateDirectory(baseDir);
            }
            string[] categories = Directory.GetDirectories(sourcePath);
            for (int i = 0; i < categories.Count(); i++)
            {
                string catDir = baseDir + "\\" + Path.GetFileName(categories[i]);
                if (!System.IO.Directory.Exists(baseDir + "\\Testing\\" + Path.GetFileName(categories[i])))
                {
                    System.IO.Directory.CreateDirectory(baseDir + "\\Testing\\" + Path.GetFileName(categories[i]));
                }
                if (!System.IO.Directory.Exists(baseDir + "\\Training\\" + Path.GetFileName(categories[i])))
                {
                    System.IO.Directory.CreateDirectory(baseDir + "\\Training\\" + Path.GetFileName(categories[i]));
                }
                string[] file_names = Directory.GetFiles(categories[i]);
                for(int j=0;j<file_names.Count();j++)
                {
                    string targetFile;
                    if (j >= (file_names.Count() / division) * (part - 1) && j < (file_names.Count() / division) * (part))
                    {
                        targetFile = baseDir + "\\Testing\\" + Path.GetFileName(categories[i]) + "\\" + Path.GetFileName(file_names[j]);
                    }
                    else 
                    {
                        targetFile = baseDir + "\\Training\\" + Path.GetFileName(categories[i]) + "\\" + Path.GetFileName(file_names[j]);
                    }
                    System.IO.File.Copy(file_names[j], targetFile, true);
                }
            }
        }
        public static double[][] gen_training_data(int docCount,string[] dictionary)
        { 
            double[][] result = new double[docCount][];
            for (int i = 0; i < docFeatureTable.Count; i++)
            {
                result[i] = new double[10000];
                for(int j = 0;j<10000;j++) //initial
                    result[i][j] = 0;
                List<string> wordList = docFeatureTable[i].Keys.Cast<string>().ToList();
                foreach (string word in wordList)
                {
                    int j = Array.IndexOf(dictionary,word);
                    if(j != -1)
                    {
                        result[i][j] = (double)docFeatureTable[i][word];
                    }
                }
            }
            return result;
        }
        static public List<testResult> RunTest(string path, string[] dictionary , Hashtable idfTable, KNearestNeighbors knn)
        { 
            List<testResult> result = new List<testResult>();
            //int[] trainingAnswer = new int[17998];
            int count = 0;
            string[] categories = Directory.GetDirectories(path);
            for (int i = 0; i < categories.Count(); i++) //traverse Categories
            {
                Console.WriteLine(Path.GetFileName(categories[i]));
                string[] file_names = Directory.GetFiles(categories[i]);
                for (int j = 0; j < file_names.Count(); j++) //file in Cagetory
                {
                    Console.WriteLine(Path.GetFileName(file_names[j]));
                    System.IO.StreamReader file = new System.IO.StreamReader(file_names[j]);
                    double[] featureV = new double[10000];
                    for(int k = 0;k<10000;k++) //initial
                        featureV[k] = 0;
                    String line;
                    int counter = 0;
                    Hashtable docWord = new Hashtable();
                    Stemmer stemmer = new Stemmer();
                    int sumWordCount = 0;
                    stemmer.stem();
                    //Console.WriteLine(stemmer.stem("running"));
                    //String word;
            
                    /******Structured Column*****/
                    while ((line = file.ReadLine()) != null)
                    {
                        //Console.WriteLine(line);
                        if (line.Contains(": "))
                        {
                            //columnCountTable[line.Split(new string[] { ": " }, StringSplitOptions.None)[0].Trim()] = file_names[j];
                        }
                        else
                        {
                            break;
                        }
                        counter++;
                    }

                    /******Text******/
                    while ((line = file.ReadLine()) != null)
                    {
                        //foreach(string iter_word in line.Split(new Char [] {' ', ',', '.', ':', '\t', '\n' }))
                        foreach (string iter_word in Regex.Split(line, @"[^A-Za-z0-9_-]"))
                        {
                            String word = iter_word.ToLower().Trim(new Char[] { '_', '-' });
                            double Num;
                            bool isNum = double.TryParse(word, out Num);
                            if (isNum)
                            {
                                continue;
                            }
                            stemmer.add(word.ToCharArray(), word.Length);
                            stemmer.stem();
                            word = stemmer.ToString();
                            if (word.Length == 0)
                            {
                                continue;
                            }
                            if (stopWordTable.ContainsKey(word))
                            {
                                continue;
                            }
                            sumWordCount++;
                            // word preprocess done
                            if (docWord.ContainsKey(word))
                            {
                                int temp = (int)docWord[word];
                                temp++;
                                docWord[word] = temp;
                            }
                            else
                            {
                                docWord[word] = 1;
                            }
                        }
                    }// line end
                    foreach (string word in docWord.Keys)
                    {
                        int indexOfDic = Array.IndexOf(dictionary, word);
                        if (indexOfDic != -1)
                        {
                            double TF = System.Convert.ToDouble((int)docWord[word])/System.Convert.ToDouble(sumWordCount);
                            double IDF = (double)idfTable[word];
                            featureV[indexOfDic] = TF * IDF;
                        }
                    }
                    testResult resultTemp = new testResult();
                    resultTemp.docName = Path.GetFileName(file_names[j]);
                    resultTemp.oriClass = i;
                    resultTemp.resultClass = knn.Compute(featureV);
                    result.Add(resultTemp);
                    Console.WriteLine(resultTemp.resultClass);
                }//file end
                //Console.ReadLine();
            }//category end
            return result;
        }
        static void Main(string[] args)
        {
            //Console.WriteLine(test());
            //String text = System.IO.File.ReadAllLines();
            Hashtable process_dictionary = new Hashtable();
            StreamReader stopFile = new StreamReader(@"D:\work\KPMG\learning\project1\stopword.txt");
            string line;
            while ((line = stopFile.ReadLine()) != null)
            {
                stopWordTable[line.Trim()] = 1;
            }
            int[] trainingAnswer = ProcessDirectory(@"D:\work\KPMG\learning\project1\test_data\1\Training");
            StreamWriter dicFile = new StreamWriter(@"D:\work\KPMG\learning\project1\dictionary_1.txt");
            Hashtable idfTable = TFIDF();
            idfTable = classIDFTable;
            string[] dictionary = gen_dic(10000);
            /*for (int i = 0; i < docFeatureTable.Count; i++)
            {
                List<string> wordList = docFeatureTable[i].Keys.Cast<string>().ToList();
                foreach (string word in wordList)
                {
                    double temp = (double)docFeatureTable[i][word];
                    if (process_dictionary.ContainsKey(word))
                    {
                        temp += (double)process_dictionary[word];
                    }
                    process_dictionary[word] = temp;
                }
            }
            List<dicWord> process_list = new List<dicWord>();
            string[] dictionary = new string[10005];
            foreach (string word in process_dictionary.Keys)
            {
                dicWord temp;
                temp.word = word;
                temp.tfidf = (double)process_dictionary[word];
                process_list.Add(temp);
                //dicFile.WriteLine(word + ": " + process_dictionary[word]);
            }
            process_list.Sort((a, b) => b.tfidf.CompareTo(a.tfidf));
            for (int i = 0; i < process_list.Count(); i++)// gen sorted dictionary
            {
                if (i >= 10000)
                    break;
                dictionary[i] = process_list[i].word;
                dicFile.WriteLine(i + ": " + process_list[i].word + ": " + process_list[i].tfidf);
            }*/
            var trainingResult = new double[trainingAnswer.Count()][];
            trainingResult = gen_training_data(trainingAnswer.Count(),dictionary);
            KNearestNeighbors knn;
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine("======================" + i + "======================");
                knn = new KNearestNeighbors(k: i, classes: 20, inputs: trainingResult, outputs: trainingAnswer);
                List<testResult> result = RunTest(@"D:\work\KPMG\learning\project1\test_data\1\Testing", dictionary, idfTable, knn);
                System.IO.Directory.CreateDirectory(@"D:\work\KPMG\learning\project1\result\" + i);
                StreamWriter resultFile = new StreamWriter(@"D:\work\KPMG\learning\project1\result\" + i + "\\test_result.csv");
                StreamWriter statisticFile = new StreamWriter(@"D:\work\KPMG\learning\project1\result\" + i + "\\test_statistic.csv");
                Hashtable classResultMap = new Hashtable();
                foreach (testResult temp in result)
                {
                    testClassResult classResult = new testClassResult();
                    classResult.correct = 0;
                    classResult.count = 0;
                    if (classResultMap.ContainsKey(temp.oriClass))
                    {
                        classResult = (testClassResult)classResultMap[temp.oriClass];
                    }
                    classResult.count++;
                    if (temp.oriClass == temp.resultClass)
                    {
                        resultFile.WriteLine(temp.docName + "," + temp.oriClass + "," + temp.resultClass + ",1");
                        classResult.correct++;
                    }
                    else
                    {
                        resultFile.WriteLine(temp.docName + "," + temp.oriClass + "," + temp.resultClass + ",0");
                    }
                    classResultMap[temp.oriClass] = classResult;
                }
                foreach (int classNo in classResultMap.Keys)
                {
                    testClassResult classResult = (testClassResult)classResultMap[classNo];
                    statisticFile.WriteLine(classNo + "," + classResult.correct + "," + classResult.count + "," + System.Convert.ToDouble(classResult.correct) / System.Convert.ToDouble(classResult.count));
                }
                resultFile.Close();
                statisticFile.Close();
            }
            //Console.WriteLine(knn.Compute(trainingResult[13366]));
            /*Console.WriteLine(Array.IndexOf(dictionary, "god"));
            Console.WriteLine(Array.IndexOf(dictionary, "nonzero"));*/
            /*for (int i = 1; i <= 10; i++)
            {
                gen_test(@"D:\work\KPMG\learning\project1\20_newsgroups", @"D:\work\KPMG\learning\project1\test_data", i, 10);
            }*/
            //Console.ReadLine();
        }


        // Process all files in the directory passed in, recurse on any directories  
        // that are found, and process the files they contain. 
        public static int[] ProcessDirectory(string targetDirectory)
        {
            //int[] trainingAnswer = new int[17998];
            List<int> trainingAnswer = new List<int>();
            int count = 0;
            string[] categories = Directory.GetDirectories(targetDirectory);
            for (int i = 0; i < categories.Count(); i++) //traverse Categories
            {
                int classWordCount = 0;
                Hashtable wordInCategory = new Hashtable();
                string[] file_names = Directory.GetFiles(categories[i]);
                for (int j = 0; j < file_names.Count(); j++) //file in Cagetory
                {
                    trainingAnswer.Add(i);
                    count++;
                    classWordCount += ProcessTraining(file_names[j], wordInCategory);
                }
                Hashtable tempWordTable = new Hashtable();
                foreach (string word in wordInCategory.Keys)//generate TF AND count word in N class
                {
                    double TF = System.Convert.ToDouble((int)wordInCategory[word]) / System.Convert.ToDouble(classWordCount);
                    tempWordTable[word] = TF;
                    int temp = 0;
                    if (classIDFTable.ContainsKey(word))//infinity?
                    {
                        temp = (int)classIDFTable[word];
                    }
                    temp++;
                    classIDFTable[word] = temp;
                }
                classWordTable.Add(tempWordTable);
            }
            Hashtable tempClassIDFTable = new Hashtable();
            foreach (string word in classIDFTable.Keys)// generate IDF value
            {
                double temp = Math.Log(System.Convert.ToDouble(classWordTable.Count()) / System.Convert.ToDouble((int)classIDFTable[word]));
                tempClassIDFTable[word] = temp;
            }
            classIDFTable = tempClassIDFTable;
            for (int i = 0; i < classWordTable.Count(); i++)
            {
                Hashtable tempWordTable = new Hashtable();
                foreach (string word in classWordTable[i].Keys)
                {
                    double temp;
                    temp = (double)classWordTable[i][word] * (double)classIDFTable[word];
                    tempWordTable[word] = temp;
                }
                classWordTable[i] = tempWordTable;
            }
            return trainingAnswer.ToArray();
        }

        // Insert logic for processing found files here. 
        public static int ProcessTraining(string filePath, Hashtable wordInCategory)
        {
            //Console.WriteLine("Processed file '{0}'.", filePath);
            //Console.WriteLine(filePath.Replace("20_newsgroups","parsed"));
            // Read the file and display it line by line.
            fileList.Add(filePath);
            System.IO.StreamReader file = new System.IO.StreamReader(filePath);
            String line;
            int counter = 0;
            Hashtable docWord = new Hashtable();
            Stemmer stemmer = new Stemmer();
            
            stemmer.stem();
            //Console.WriteLine(stemmer.stem("running"));
            //String word;
            
            /******Structured Column*****/
            while ((line = file.ReadLine()) != null)
            {
                //Console.WriteLine(line);
                if (line.Contains(": "))
                {
                    columnCountTable[line.Split(new string[] { ": " }, StringSplitOptions.None)[0].Trim()] = filePath;
                }
                else
                {
                    break;
                }
                counter++;
            }
            counter = 0;
            /******Text******/
            while ((line = file.ReadLine()) != null)
            {
                //foreach(string iter_word in line.Split(new Char [] {' ', ',', '.', ':', '\t', '\n' }))
                foreach (string iter_word in Regex.Split(line, @"[^A-Za-z0-9_-]"))
                {
                    wordCount temp;
                    int wordCountTemp = 0;
                    String word = iter_word.ToLower().Trim(new Char[] { '_', '-' });
                    double Num;
                    bool isNum = double.TryParse(word, out Num);
                    if(isNum)
                    {
                        continue;
                    }
                    stemmer.add(word.ToCharArray(), word.Length);
                    stemmer.stem();
                    word = stemmer.ToString();
                    if (word.Length == 0)
                    {
                        continue;
                    }
                    if (stopWordTable.ContainsKey(word))
                    {
                        continue;
                    }
                    // word preprocess done
                    counter++;
                    if (wordInCategory.ContainsKey(word))
                    {
                        int count = (int)wordInCategory[word];
                        count += 1;
                        wordInCategory[word] = count;
                    }
                    else 
                    {
                        wordInCategory[word] = 1;
                    }
                    if (wordCountTable.ContainsKey(word)) //word already apper
                    {
                        temp = (wordCount)wordCountTable[word];
                        temp.count += 1;
                        if (!docWord.ContainsKey(word))//add DF
                        {
                            temp.DF += 1;
                        }
                    }
                    else
                    {
                        temp.count = 1;
                        temp.DF = 1;
                    }
                    if (docWord.ContainsKey(word))
                    {
                        wordCountTemp = (int)docWord[word];
                    }
                    wordCountTemp += 1;
                    docWord[word] = wordCountTemp;
                    wordCountTable[word] = temp;
                }
            }
            docFeatureTable.Add(docWord);
            file.Close();
            //Console.ReadLine();
            return counter;
        }
    }
}

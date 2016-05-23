﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;
using LibSVMsharp.Extensions;
using LibSVMsharp.Core;
using BesterUI.Data;
using BesterUI;
using LibSVMsharp.Helpers;
namespace Classification_App
{
    class OneClassClassifier
    {
        List<List<double>> _trainingData;
        SVMModel _model;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="trainingData">The training data which the svm will learn the "normal" data from</param>
        public OneClassClassifier(List<List<double>> trainingData)
        {
            _trainingData = trainingData;
        }

        /// <summary>
        /// Create a model from the training data, has to be called before predicting
        /// </summary>
        /// <param name="paramater">The SvmParameter is converted to OneClass type</param>
        /// <returns>returns true if the model was created correctly</returns>
        public bool CreateModel(SVMParameter parameter)
        {
            parameter.Type = SVMType.ONE_CLASS;

            try
            {
                SVMProblem problem = _trainingData.CreateCompleteProblemOneClass();
                _model = problem.Train(parameter);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Find the outliers in a dataset given the internal model
        /// (call after CreateModel)
        /// </summary>
        /// <param name="data">Data to find outliers in</param>
        /// <returns>indexing of where outliers were found in the list</returns>
        public List<OneClassFV> PredictOutliers(List<OneClassFV> data)
        {
            if (_model == null)
            {
                //Remember to call CreateModel before PredictData
                return null;
            }

            List<SVMNode[]> nodeSets = data.Select(x=>x.Features).CreateNodesFromData();
            List<int> results = new List<int>();

            for (int i = 0; i < nodeSets.Count; i++)
            {
                double result = _model.Predict(nodeSets[i]);
                if (result == -1)
                {
                    results.Add(i);
                }
            }

            List<OneClassFV> resultList = new List<OneClassFV>();
            foreach(int result in results)
            {
                resultList.Add(data[result]);
            }
            return resultList;
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

/// <summary>
/// Esta classe permite a execu��o de um modelo treinado de Pix2Pix para
/// realizer infer�ncias a partir de imagens novas
/// </summary>
public class Pix2Pix
{
    #region Campos e propriedades

    NNModel _modelAsset;
    Model _loadedModel;
    IWorker _worker;

    #endregion

    #region Construtor

    /// <summary>
    /// Construtor de um objeto de infer�ncia Pix2pix
    /// </summary>
    public Pix2Pix()
    {
        _modelAsset = Resources.Load<NNModel>("NeuralModels/treinado");
        _loadedModel = ModelLoader.Load(_modelAsset);
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, _loadedModel);
    }

    #endregion

    #region M�todos p�blicos


    #endregion

    #region M�todos privados

    /// <summary>
    /// Traduz um tensor em Texture2D
    /// </summary>
    /// <param name="inputTensor"></param>
    /// <param name="inputTexture"></param>
    /// <returns></returns>
    Texture2D Tensor2Image(Tensor inputTensor, Texture2D inputTexture)
    {
        var tempRT = new RenderTexture(256, 256, 24);
        inputTensor.ToRenderTexture(tempRT);
        RenderTexture.active = tempRT;

        var resultTexture = new Texture2D(inputTexture.width, inputTexture.height);
        resultTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        resultTexture.Apply();

        RenderTexture.active = null;
        tempRT.DiscardContents();

        return resultTexture;
    }

    /// <summary>
    /// Normaliza um tensor para um campo determinado
    /// </summary>
    /// <param name="inputTensor"></param>
    /// <param name="a1">M�nimo original</param>
    /// <param name="a2">M�ximo original</param>
    /// <param name="b1">M�nimo esperado</param>
    /// <param name="b2">M�ximo esperado</param>
    /// <returns></returns>
    Tensor NormaliseTensor(Tensor inputTensor, float a1, float a2, float b1, float b2)
    {
        var data = inputTensor.data.Download(inputTensor.shape);
        float[] normalized = new float[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            normalized[i] = Util.Normalise(data[i], a1, a2, b1, b2);
        }

        return new Tensor(inputTensor.shape, normalized);
    }

    #endregion
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Determina o estágio atual da aplicação
/// </summary>
public enum AppStage { Neutral = 0, Selecting = 1, Done = 2 }

/// <summary>
/// Classe que gerencia o ambiente da aplicação
/// </summary>
public class EnvironmentManager : MonoBehaviour
{
    #region Campos privados

    VoxelGrid _grid;
    Voxel[] _corners;
    AppStage _stage;
    /// <summary>Determina a altura atual da caixas a serem criadas</summary>
    int _height;

    /// <summary>Seed para controle dos números aleatórios</summary>
    int _seed = 666;

    Texture2D _sourceImage;
    UIManager _uiManager;

    #endregion

    #region Métodos do Unity
    void Start()
    {
        // Coleta o UIManager
        _uiManager = GameObject.Find("UIManager").transform.GetComponent<UIManager>();
        if (_uiManager == null) Debug.LogError("UIManager não foi encontrado!");
        
        // Define as imagens que podem ser utilizadas manualmente
        _sourceImage = _uiManager.SetDropdownSources();

        // Inicializa o motor de números aleatórios e a aplicação
        Random.InitState(_seed);
        _stage = AppStage.Neutral;

        // Inicializa o grid que será trabalhado
        _corners = new Voxel[2];
        var gridSize = _uiManager.GridSize;
        _height = gridSize.y;
        var maxSize = _uiManager.MaxGridSize;
        _grid = new VoxelGrid(gridSize, maxSize, transform.position, 1f, transform);

    }

    void Update()
    {
        HandleDrawing();
        HandleHeight();
        // Limpar o grid utilizando a tecla "C"
        if (Input.GetKeyDown(KeyCode.C)) _grid.ClearGrid();
        
    }

    #endregion

    /// <summary>
    /// Expõe o método de criação do set de treinamento para um botão
    /// </summary>
    public void GenerateSampleSet()
    {

    }


    public void SaveGrid(string name)
    {
        if (name == "")
        {
            Debug.Log("Não é possível salvar arquivo sem nome!");
            return;
        }
        var directory = Path.Combine(Directory.GetCurrentDirectory(), $"Grids");
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"{name}.csv");
        Util.SaveVoxels(_grid, new List<VoxelState>() { VoxelState.Red, VoxelState.Black }, path);

    }

    /// <summary>
    /// Expõe a função de atualizar os voxels com estado <see cref="VoxelState.Red"/>
    /// </summary>
    public void UpdateReds()
    {
        _grid.ClearReds();
        _grid.SetStatesFromImage(_sourceImage,
            _uiManager.GetSturctureBase(),
            _uiManager.GetSturctureTop(),
            _uiManager.GetSturctureThickness(),
            _uiManager.GetSturctureSensitivity(),
            setBlacks: false);
    }

    /// <summary>
    /// Gerencia o processo de desenho de caixas na interface
    /// </summary>
    private void HandleDrawing()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {

                if (hit.transform.CompareTag("Voxel"))
                {
                    Voxel voxel = hit.transform.GetComponent<VoxelTrigger>().Voxel;
                    if (voxel.State == VoxelState.White || voxel.State == VoxelState.Yellow)
                    {
                        if (_stage == AppStage.Neutral)
                        {
                            _uiManager.SetMouseTagText(_height.ToString());
                            _stage = AppStage.Selecting;
                            _corners[0] = voxel;
                            voxel.SetState(VoxelState.Black);
                        }
                        else if (_stage == AppStage.Selecting && voxel != _corners[0])
                        {
                            _corners[1] = voxel;
                            _grid.SetCorners(_corners);
                        }
                    }
                }
            }
        }
        if (_stage == AppStage.Selecting)
        {
            _uiManager.SetMouseTagPosition(Input.mousePosition + new Vector3(50, 0, 15));
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (_stage == AppStage.Selecting) _stage = AppStage.Done;
            _uiManager.HideMouseTag();
        }
        if (_stage == AppStage.Done)
        {
            _grid.MakeBox(_height);
            _stage = AppStage.Neutral;
        }
    }

    /// <summary>
    /// Gerencia o controle da altura das caixas a serem criadas na interface
    /// </summary>
    private void HandleHeight()
    {
        if (Input.GetKeyDown(KeyCode.Period))
        {
            _height = Mathf.Clamp(_height + 1, 1, _grid.Size.y);
            _uiManager.SetMouseTagText(_height.ToString());
        }
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            _height = Mathf.Clamp(_height - 1, 1, _grid.Size.y);
            _uiManager.SetMouseTagText(_height.ToString());
        }
    }

    /// <summary>
    /// Atualiza o tamanho do grid
    /// </summary>
    /// <param name="newSize"></param>
    public void UpdateGridSize(Vector3Int newSize)
    {
        _height = Mathf.Clamp(_height, 1, newSize.y);
        _grid.ChangeGridSize(newSize);
        _grid.ShowPreview(false);
    }

    /// <summary>
    /// Executa a previsão do novo tamanho do grid
    /// </summary>
    /// <param name="newSize"></param>
    public void PreviewGridSize(Vector3Int newSize)
    {
        _grid.ShowPreview(true);
        _grid.UpdatePreview(newSize);
    }

    /// <summary>
    /// Expõe a função de ler uma imagem para a UI
    /// </summary>
    public void ReadImage()
    {
        _grid.ClearGrid();
        _grid.SetStatesFromImage(
            _sourceImage, 
            _uiManager.GetSturctureBase(), 
            _uiManager.GetSturctureTop(), 
            _uiManager.GetSturctureThickness(), 
            _uiManager.GetSturctureSensitivity());
    }

    

    /// <summary>
    /// Expõe a funlçao de limpar o grid para a UI
    /// </summary>
    public void ClearGrid()
    {
        _grid.ClearGrid();
    }

    /// <summary>
    /// Expõe a função de atualizar a imagem atual pela UI
    /// </summary>
    public void UpdateCurrentImage()
    {
        _sourceImage = _uiManager.GetCurrentImage();
    }
}

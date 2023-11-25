using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileBuilder : MonoBehaviour
{
    [SerializeField] private float _cameraSpeed = 5;

    [SerializeField] private Tilemap _cursorMap;
    [SerializeField] private TileBase _cursorTile;

    [SerializeField] private Tilemap _selectedMap;
    [SerializeField] private Tilemap _emptyMapPrefab;
    [SerializeField] private TileBase _tileToSet;

    [SerializeField] private Button _selectedTileButton;
    [SerializeField] private TextMeshProUGUI _tileMapLayerLabel;

    private List<Tilemap> _maps = new List<Tilemap>();

    private int _selectedLayer = 0;

    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;

        _selectedLayer = _selectedMap.GetComponent<TilemapRenderer>().sortingOrder;

        _maps.Add( _selectedMap );

        SetMapLayer();

        StartCoroutine(CursorTileView());

        SetButtonColor(_selectedTileButton);
    }

    private void SetMapLayer()
    {
        _selectedMap = _maps[_selectedLayer];
        _tileMapLayerLabel.text = $"Слой: {_selectedLayer}";
    }

    private void MapLayerUp()
    {
        _selectedLayer++;

        if (_maps.Count <= _selectedLayer)
        {
            var newMap = Instantiate(_emptyMapPrefab, transform);
            newMap.GetComponent<TilemapRenderer>().sortingOrder = _selectedLayer;

            _maps.Add(newMap);
        }

        SetMapLayer();
    }

    private void MapLayerDown()
    {
        if (_selectedLayer == 0)
            return;

        _selectedLayer--;

        SetMapLayer();
    }

    private void CameraMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        _camera.transform.position += _cameraSpeed * Time.deltaTime * new Vector3(x, y);
    }

    private void CameraSize()
    {
        float plusSize = 0;

        if (Input.GetKey(KeyCode.E))
        {
            plusSize = 2.5f;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            plusSize = -2.5f;
        }

        float newSize = _camera.orthographicSize += plusSize * Time.deltaTime;

        _camera.orthographicSize = Mathf.Clamp(newSize, 3f, 30f);
    }

    private void ClearAllLayers()
    {
        _selectedLayer = 0;

        for (int i = 1; i < _maps.Count; i++)
        {
            Destroy(_maps[i].gameObject);
        }

        _maps[0].ClearAllTiles();
        SetMapLayer();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Backspace))
        {
            ClearAllLayers();
        }

        if (Input.GetKeyUp(KeyCode.RightShift) ||  Input.GetKeyUp(KeyCode.LeftShift))
        {
            MapLayerUp();
        }
        else if (Input.GetKeyUp(KeyCode.RightControl) || Input.GetKeyUp(KeyCode.LeftControl))
        {
            MapLayerDown();
        }

        if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 worldPoint = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePosition = _selectedMap.WorldToCell(worldPoint);

            if (Input.GetMouseButton(0))
            {
                _selectedMap.SetTile(tilePosition, _tileToSet);
            }
            else if (Input.GetMouseButton(1))
            {
                _selectedMap.SetTile(tilePosition, null);
            }
        }

        CameraMovement();
        CameraSize();
    }

    private IEnumerator CursorTileView()
    {
        while (true)
        {
            Vector2 worldPoint = _camera.ScreenToWorldPoint(Input.mousePosition);

            var cursorCellPos = _cursorMap.WorldToCell(worldPoint);

            _cursorMap.GetComponent<TilemapRenderer>().sortingOrder = _selectedLayer;

            _cursorMap.SetTile(cursorCellPos, _cursorTile);

            yield return new WaitForSeconds(0.01f);

            _cursorMap.ClearAllTiles();
        }
    }

    public void SetTile(Button button)
    {
        _selectedTileButton.colors = button.colors;

        _selectedTileButton = button;

        SetButtonColor(_selectedTileButton);

        _tileToSet = Resources.Load<TileBase>($"Tiles/blocks/{_selectedTileButton.image.sprite.name}");
    }

    private void SetButtonColor(Button button)
    {
        var selectedColors = button.colors;
        selectedColors.normalColor = selectedColors.pressedColor;

        button.colors = selectedColors;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnvironmentGeneratorServerSide : MonoBehaviour
{
    public static Dictionary<int, GameObject> planets = new Dictionary<int, GameObject>();
    public static Dictionary<int, GameObject> nonGravityObjectDict = new Dictionary<int, GameObject>();
    public static Dictionary<int, Vector3> spawnPoints = new Dictionary<int, Vector3>();
    public static int BoundaryDistanceFromOrigin;

    public int minPositionX, maxPositionX, minPositionY, maxPositionY, minPositionZ, maxPositionZ;
    // Planets
    public int planetMinScale, planetMaxScale;
    public int minPlanetCount, maxPlanetCount;
    public float minPlanetDistance, maxPlanetDistance;
    // Non gravity objects
    public int nonGravityObjectMinScale, nonGravityObjectMaxScale;
    public int minNonGravityObjectCount, maxNonGravityObjectCount;
    public float minNonGravityObjectDistance, maxNonGravityObjectDistance;

    public GameObject planetBase;
    public GameObject[] nonGravityObjects;

    private void Start()
    {
        //SceneManager.LoadSceneAsync("Client", LoadSceneMode.Additive);
        BoundaryDistanceFromOrigin = Mathf.Max(new int[] {
                                        Mathf.Abs(minPositionX) + Mathf.Abs(maxPositionX),
                                        Mathf.Abs(minPositionY) + Mathf.Abs(maxPositionY),
                                        Mathf.Abs(minPositionZ) + Mathf.Abs(maxPositionZ)
                                        }) + 100;
        Random.InitState(Random.Range(0, 10000));
        StartCoroutine(GeneratePlanets());
    }

    private IEnumerator GeneratePlanets()
    {
        int _errorCatcher, _maxErrorCatcher = 10000,_planetCount = Random.Range(minPlanetCount, maxPlanetCount);
        float _planetScale; 
        Vector3 _planetPosition;
        int _index = 0;

        for (int i = 0; i < _planetCount; i++)
        {
            _errorCatcher = 0;
            do
            {
                if (_errorCatcher >= _maxErrorCatcher)
                    yield return new WaitForEndOfFrame();
                _planetScale = Random.Range(planetMinScale, planetMaxScale);
                _planetPosition = GenerateRandomVector();
                _errorCatcher++;
            }
            while (ColliderInPosition(_planetScale, _planetPosition));

            GameObject _planet = Instantiate(planetBase, _planetPosition, Quaternion.identity);
            _planet.transform.localScale = new Vector3(_planetScale, _planetScale, _planetScale);

            planets.Add(_index, _planet);
            spawnPoints.Add(_index, new Vector3(_planet.transform.position.x,
                                               _planet.transform.position.y + _planet.transform.localScale.y / 2 + 1,
                                               _planet.transform.position.z
                                               ));
            _index++;

            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(GenerateNonGravityObjects());
    }

    private IEnumerator GenerateNonGravityObjects()
    {
        int _errorCatcher, _maxErrorCatcher = 10000, _index = 0;
        int _objectCount = Random.Range(minNonGravityObjectCount, maxNonGravityObjectCount);
        Vector3 _objectPosition, _objectScale;

        for (int i = 0; i < _objectCount; i++)
        {
            _errorCatcher = 0;
            do
            {
                if (_errorCatcher >= _maxErrorCatcher)
                {
                    yield return new WaitForEndOfFrame();
                }
                _objectScale = new Vector3(
                    Random.Range(nonGravityObjectMinScale, nonGravityObjectMaxScale),
                    Random.Range(nonGravityObjectMinScale, nonGravityObjectMaxScale),
                    Random.Range(nonGravityObjectMinScale, nonGravityObjectMaxScale));
                _objectPosition = GenerateRandomVector();
                _errorCatcher++;
            }
            while (ColliderInPosition(_objectScale, _objectPosition));

            GameObject _object = Instantiate(nonGravityObjects[Random.Range(0, nonGravityObjects.Length)]
                                             ,_objectPosition, Quaternion.identity);
            _object.transform.localScale = _objectScale;
            _object.transform.rotation = GenerateRandomQuaternion();

            nonGravityObjectDict.Add(_index, _object);
            _index++;

            yield return new WaitForEndOfFrame();
        }
    }

    private bool ColliderInPosition(float _scale, Vector3 _position)
    {
        float _planetDistance = Random.Range(minPlanetDistance, maxPlanetDistance);
        return Physics.CheckSphere(_position, _scale * _planetDistance);
    }

    private bool ColliderInPosition(Vector3 _scale, Vector3 _position)
    {
        float _objectDistance = Random.Range(minNonGravityObjectDistance, maxNonGravityObjectDistance);
        return Physics.CheckBox(_position, _scale * _objectDistance);
    }

    private Vector3 GenerateRandomVector()
    {
        return new Vector3(Random.Range(minPositionX, maxPositionX), Random.Range(minPositionY, maxPositionY), Random.Range(minPositionZ, maxPositionZ));
    }

    private Quaternion GenerateRandomQuaternion()
    {
        return Quaternion.Euler(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f));
    }
}

using UnityEngine;

public class PerlinScroller : MonoBehaviour {
  private int cubeCount;
  public int width = 500;
  public int height = 500;
  public int layers = 3;

  private GameObject[] cubes;

  private void Awake() {
    cubeCount = width * height * layers;
  }

  private void Start() {
    cubes = CreateCubes(cubeCount);
  }

  public GameObject[] CreateCubes(int count) {
    var cubes = new GameObject[count];
    var cubeToCopy = new GameObject();
    for(int i = 0; i < count; i++) {
      var cube = GameObject.Instantiate(cubeToCopy);
      int x = i / (width * layers);
      cube.transform.position = new Vector3(x, 0, (i - x * height * layers) / layers);
      cubes[i] = cube;
    }

    Destroy(cubeToCopy);
    return cubes;
  }

  private int xOffset = 0;
  private void Update() {
    //int xOffset = (int)(transform.position.x - width * 0.5f);
    xOffset++;
    int zOffset = (int)(transform.position.z - height * 0.5f);
    for(int i = 0; i < cubeCount; i++) {
      int x = i / (width * layers);
      int z = (i - x * height * layers) / layers;
      int yOffset = i - x * width * layers - z * layers;
      cubes[i].transform.position = new Vector3(
        x /*+ xOffset*/,
        GeneratePerlinHeight(x + xOffset, z + zOffset) + yOffset,
        z + zOffset);
    }

    if (Input.GetKey("up")) transform.Translate(0, 0, 2);
    else if (Input.GetKey("down")) transform.Translate(0, 0, -2);
    else if (Input.GetKey("left")) transform.Translate(-2, 0, 0);
    else if (Input.GetKey("right")) transform.Translate(2, 0, 0);
  }

  public float GeneratePerlinHeight(float posX, float posZ) {
    float smooth = 0.03f;
    float heightMult = 5;
    float height = (Mathf.PerlinNoise(posX * smooth, posZ * smooth * 2) * heightMult +
                    Mathf.PerlinNoise(posX * smooth, posZ * smooth * 2) * heightMult) / 2.0f;
    return height * 10;
  }
}

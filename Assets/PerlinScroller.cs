using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

public class PerlinScroller : MonoBehaviour {
  private int cubeCount;
  public int width = 500;
  public int height = 500;
  public int layers = 3;

  private GameObject[] cubes;
  private TransformAccessArray cubeTransformAccessArray;
  private PositionUpdateJob cubeJob;
  private JobHandle cubePositionJobHandle;

  private void Awake() {
    cubeCount = width * height * layers;
  }

  private void Start() {
    cubes = CreateCubes(cubeCount);
    var cubeTransforms = new Transform[cubeCount];
    for (int i = 0; i < cubeCount; i++) {
      cubeTransforms[i] = cubes[i].transform;
    }
    cubeTransformAccessArray = new TransformAccessArray(cubeTransforms);
  }

  public GameObject[] CreateCubes(int count) {
    var cubes = new GameObject[count];
    var cubeToCopy = new GameObject();
    for (int i = 0; i < count; i++) {
      var cube = GameObject.Instantiate(cubeToCopy);
      int x = i / (width * layers);
      cube.transform.position = new Vector3(x, 0, (i - x * height * layers) / layers);
      cubes[i] = cube;
    }

    Destroy(cubeToCopy);
    return cubes;
  }

  [BurstCompile]
  struct PositionUpdateJob : IJobParallelForTransform {
    public int height;
    public int width;
    public int layers;
    public int xOffset;
    public int zOffset;

    public void Execute(int i, TransformAccess transform) {
      int x = i / (width * layers);
      int z = (i - x * height * layers) / layers;
      int yOffset = i - x * width * layers - z * layers;
      transform.position = new Vector3(
        x /*+ xOffset*/,
        GeneratePerlinHeight(x + xOffset, z + zOffset) + yOffset,
        z + zOffset);
    }
  }

  private int xOffset = 0;
  private void Update() {
    cubeJob = new PositionUpdateJob() {
      xOffset = xOffset++,
      zOffset = (int)(transform.position.z - height * 0.5f),
      height = height,
      width = width,
      layers = layers
    };

    cubePositionJobHandle = cubeJob.Schedule(cubeTransformAccessArray);
    JobHandle.ScheduleBatchedJobs();

    if (Input.GetKey("up")) transform.Translate(0, 0, 2);
    else if (Input.GetKey("down")) transform.Translate(0, 0, -2);
    else if (Input.GetKey("left")) transform.Translate(-2, 0, 0);
    else if (Input.GetKey("right")) transform.Translate(2, 0, 0);
  }

  public void LateUpdate() {
    cubePositionJobHandle.Complete();
  }

  public void OnDestroy() {
    cubeTransformAccessArray.Dispose();
  }

  public static float GeneratePerlinHeight(float posX, float posZ) {
    float smooth = 0.03f;
    float heightMult = 5;
    float height = (Mathf.PerlinNoise(posX * smooth, posZ * smooth * 2) * heightMult +
                    Mathf.PerlinNoise(posX * smooth, posZ * smooth * 2) * heightMult) / 2.0f;
    return height * 10;
  }
}

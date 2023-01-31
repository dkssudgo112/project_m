using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 22.07.25_jayjeong
/// <br> 오브젝트풀 매니저 : Item 클래스를 상속받는 모든 아이템 생성 및 제거 관리 </br>
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
	private static ObjectPoolManager Instance = null;

	// 오브젝트 풀 단위 클래스 
	[Serializable]
	public class Pool
	{
		public string name;
		public GameObject prefab;
		public int size;
	}

	// 오브젝트풀 리스트 
	[SerializeField]
	private Pool[] _pools = null;
	// 스폰된 오브젝트 리스트 
	private List<GameObject> _spawnedObjects = new List<GameObject>();
	// 각 오브젝트 단위의 오브젝트풀 큐 딕셔너리 
	private Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();
	// 모든 아이템을 viewID를 Key 값으로 관리하는 딕셔너리 
	private Dictionary<int, GameObject> _itemPoolDictionary = new Dictionary<int, GameObject>();
	public int _viewID = 0;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}
    private void Start()
	{
		CreateObjectPoolsOnStart();
	}
    
	#region Item 클래스를 상속받은 GameObject 생성 함수 

    public static GameObject AllocItem(string name, Vector3 position) =>
		Instance.AllocItemFromPool(name, position, Quaternion.identity);

	public static GameObject AllocItem(string name, Vector3 position, Quaternion rotation) =>
		Instance.AllocItemFromPool(name, position, rotation);

	public static T AllocItem<T>(string name, Vector3 position) where T : Component
	{
		GameObject obj = Instance.AllocItemFromPool(name, position, Quaternion.identity);

		if (obj.TryGetComponent(out T component))
        {
			return component;
        }
		else
        {
			obj.SetActive(false);
			throw new Exception($"Component is not found");
		}
	}

	public static T AllocItem<T>(string name, Vector3 position, Quaternion rotation) where T : Component
	{
		GameObject obj = Instance.AllocItemFromPool(name, position, rotation);

		if (obj.TryGetComponent(out T component))
        {
			return component;
		}
		else
		{
			obj.SetActive(false);
			throw new Exception($"Component is not found");
		}
	}

	private GameObject AllocItemFromPool(string name, Vector3 position, Quaternion rotation)
	{
		if (name == null)
        {
			throw new Exception($"The item name is null");
		}

		if (_poolDictionary.ContainsKey(name) == false)
        {
			throw new Exception($"Pool with name {name} doesn't exist");
		}

		Queue<GameObject> poolQueue = _poolDictionary[name];

		// 해당 name의 큐가 비어있다면 새로 추가 
		if (poolQueue.Count <= 0)
		{
			var obj = CreateNewItem(name, ItemManager.GetItemByName(name).gameObject);

			if (obj != null)
            {
				ArrangePool(obj);
			}
			else
            {
				throw new Exception($"Item with name {name} doesn't exist in itemDictionary.");
            }
		}

		// 큐에 오브젝트가 존재한다면 꺼내서 사용 
		GameObject objToSpawn = poolQueue.Dequeue();
		objToSpawn.transform.position = position;
		objToSpawn.transform.rotation = rotation;
		objToSpawn.SetActive(true);

		return objToSpawn;
	}

    #endregion

    #region Pool 클래스를 이용한 GameObject 생성 함수 

    public static GameObject AllocObject(string name, Vector3 position) =>
	Instance.AllocObjectFromPool(name, position, Quaternion.identity);

	public static GameObject AllocObject(string name, Vector3 position, Quaternion rotation) =>
		Instance.AllocObjectFromPool(name, position, rotation);

	public static T AllocObject<T>(string name, Vector3 position) where T : Component
	{
		GameObject obj = Instance.AllocObjectFromPool(name, position, Quaternion.identity);

		if (obj.TryGetComponent(out T component))
        {
			return component;
		}
		else
		{
			obj.SetActive(false);
			throw new Exception($"Component is not found");
		}
	}

	public static T AllocObject<T>(string name, Vector3 position, Quaternion rotation) where T : Component
	{
		GameObject obj = Instance.AllocObjectFromPool(name, position, rotation);

		if (obj.TryGetComponent(out T component))
        {
			return component;
		}
		else
		{
			obj.SetActive(false);
			throw new Exception($"Component is not found");
		}
	}

	private GameObject AllocObjectFromPool(string name, Vector3 position, Quaternion rotation)
	{
		if(name == null)
        {
			throw new Exception($"The object name is null");
        }

		if(_poolDictionary.ContainsKey(name) == false)
        {
			throw new Exception($"Pool with name {name} doesn't exist.");
        }

		Queue<GameObject> poolQueue = _poolDictionary[name];

		// 해당 name의 큐가 비어있다면 새로 추가 
		if (poolQueue.Count <= 0)
		{
			Pool pool = Array.Find(_pools, x => x.name == name);

			if (pool == null)
			{
				throw new Exception($"The pool with {name} does not exist in the Pools list");
			}

			var obj = CreateNewObject(pool.name, pool.prefab);

			if (obj != null)
            {
				ArrangePool(obj);
			}
			else
            {
				throw new Exception($"Creating New object {name} has failed.");
            }
		}

		// 큐에 오브젝트가 존재한다면 꺼내서 사용 
		GameObject objToSpawn = poolQueue.Dequeue();
		objToSpawn.transform.position = position;
		objToSpawn.transform.rotation = rotation;
		objToSpawn.SetActive(true);

		return objToSpawn;
	}

    #endregion

    /// <summary>
    /// name과 동일한 이름의 모든 pool을 List 형태로 반환 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static List<GameObject> GetAllPools(string name)
	{
		if(Instance._poolDictionary.ContainsKey(name) == false)
        {
			throw new Exception($"Pool with name {name} doesn't exist.");
        }

		return Instance._spawnedObjects.FindAll(x => x.name == name);
	}

	/// <summary>
	/// name과 동일한 이름의 모든 pool을 T 타입으로 변환해 List 형태로 반환 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="name"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public static List<T> GetAllPools<T>(string name) where T : Component
	{
		List<GameObject> objects = GetAllPools(name);

		if (objects[0].TryGetComponent(out T component) == false)
        {
			throw new Exception("Component is not found");
		}

		return objects.ConvertAll(x => x.GetComponent<T>());
	}

	public static GameObject GetItemByViewID(int viewID)
    {
		return Instance._itemPoolDictionary[viewID];
    }

	// 사용이 끝난 오브젝트를 Pool로 반환 
	public static void FreeObjectToPool(GameObject obj)
	{
		if (Instance._poolDictionary.ContainsKey(obj.name) == false)
        {
			throw new Exception($"Pool with name {obj.name} doesn't exist.");
		}

		Instance._poolDictionary[obj.name].Enqueue(obj);
	}

	[ContextMenu("GetAllocatedObjectsInfo")]
	private void GetAllocatedObjectsInfo()
	{
		foreach (var key in ItemManager.GetItemDic().Keys)
		{
			int count = _spawnedObjects.FindAll(x => x.name == key).Count;
			Debug.Log($"{key} count : {count}");
		}
	}

	// Json을 통해 데이터 파싱 시 각 아이템별 Pool 생성 
	public static void AddNewItemPoolToPoolDic(string name, GameObject item)
	{
		if(item == null)
        {
			throw new Exception("The item name is null");
        }

		Instance._poolDictionary.Add(name, new Queue<GameObject>());

		item.name = name;
		item.transform.SetParent(Instance.transform);
		item.SetActive(false);
		Instance.ArrangePool(item);
	}

	// Start 시점에 ItemDictionary 내 모든 아이템풀을 Size 만큼 생성 
	public static void CreateItemPoolsOnStart()
	{
		foreach (var item in ItemManager.GetItemDic())
		{
			Instance._poolDictionary.Add(item.Key, new Queue<GameObject>());

			if (Instance._poolDictionary.ContainsKey(item.Key) == false)
            {
				throw new Exception($"Pool with name {item.Key} doesn't exist");
			}

			for (int i = 0; i < item.Value.stackSize; i++)
			{
				var obj = Instance.CreateNewItem(item.Key, item.Value.gameObject);

				if (obj != null)
                {
					Instance.ArrangePool(obj);
				}
				else
				{
					throw new Exception($"Creating New object {item.Key} has failed.");
				}
			}

			// ReturnToPool 중복구현 검사
			if (Instance._poolDictionary[item.Key].Count != item.Value.stackSize)
				Debug.LogError($"{item.Key}에 ReturnToPool이 중복됩니다.");
		}
	}

	// Start 시점에 pools 배열의 모든 요소 Pool 생성 
	public void CreateObjectPoolsOnStart()
	{
		foreach (Pool pool in _pools)
		{
			_poolDictionary.Add(pool.name, new Queue<GameObject>());
			
			for (int i = 0; i < pool.size; i++)
			{
				var obj = CreateNewObject(pool.name, pool.prefab);

				if (obj != null)
				{
					Instance.ArrangePool(obj);
				}
				else
				{
					throw new Exception($"Creating New object {pool.name} has failed.");
				}
			}

			// ReturnToPool 중복구현 검사
			if (_poolDictionary[pool.name].Count != pool.size)
            {
				Debug.LogError($"{pool.name}에 ReturnToPool이 중복됩니다");
			}
		}
	}

	private GameObject CreateNewItem(string name, GameObject prefab)
	{
		var obj = Instantiate(prefab, transform);
		obj.name = name;
		obj.GetComponent<Item>().viewID = ++_viewID;
		_itemPoolDictionary.Add(obj.GetComponent<Item>().viewID, obj);

		// 비활성화시 ReturnToPool을 하므로 Enqueue가 됨
		obj.SetActive(false);

		return obj;
	}

	private GameObject CreateNewObject(string name, GameObject prefab)
	{
		var obj = Instantiate(prefab, transform);
		obj.name = name;

		// 비활성화시 ReturnToPool을 하므로 Enqueue가 됨
		obj.SetActive(false); 

		return obj;
	}

	private void ArrangePool(GameObject obj)
	{
		// 추가된 오브젝트 묶어서 정렬
		bool isFind = false;

		for (int i = 0; i < transform.childCount; i++)
		{
			if (i == transform.childCount - 1)
			{
				obj.transform.SetSiblingIndex(i);
				_spawnedObjects.Insert(i, obj);
				break;
			}
			else if (transform.GetChild(i).name == obj.name)
            {
				isFind = true;
			}
			else if (isFind)
			{
				obj.transform.SetSiblingIndex(i);
				_spawnedObjects.Insert(i, obj);
				break;
			}
		}
	}
}

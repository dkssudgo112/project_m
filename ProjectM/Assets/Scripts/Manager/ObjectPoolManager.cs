using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 22.07.25_jayjeong
/// <br> ������ƮǮ �Ŵ��� : Item Ŭ������ ��ӹ޴� ��� ������ ���� �� ���� ���� </br>
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
	private static ObjectPoolManager Instance = null;

	// ������Ʈ Ǯ ���� Ŭ���� 
	[Serializable]
	public class Pool
	{
		public string name;
		public GameObject prefab;
		public int size;
	}

	// ������ƮǮ ����Ʈ 
	[SerializeField]
	private Pool[] _pools = null;
	// ������ ������Ʈ ����Ʈ 
	private List<GameObject> _spawnedObjects = new List<GameObject>();
	// �� ������Ʈ ������ ������ƮǮ ť ��ųʸ� 
	private Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();
	// viewID�� Key ������ ��� �������� �����ϴ� ��ųʸ� 
	private Dictionary<int, GameObject> _itemPoolDictionary = new Dictionary<int, GameObject>();
	// ��� �����ۿ� �����Ǵ� viewID 
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
    
	#region Item Ŭ������ ��ӹ��� GameObject ���� �Լ� 

    public static GameObject AllocItem(string name, int viewID, Vector3 position) =>
		Instance.AllocItemFromPool(name, viewID, position, Quaternion.identity);

	public static GameObject AllocItem(string name, int viewID, Vector3 position, Quaternion rotation) =>
		Instance.AllocItemFromPool(name, viewID, position, rotation);

	public static T AllocItem<T>(string name, int viewID, Vector3 position) where T : Component
	{
		GameObject obj = Instance.AllocItemFromPool(name, viewID, position, Quaternion.identity);

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

	public static T AllocItem<T>(string name, int viewID, Vector3 position, Quaternion rotation) where T : Component
	{
		GameObject obj = Instance.AllocItemFromPool(name, viewID, position, rotation);

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

	private GameObject AllocItemFromPool(string name, int viewID, Vector3 position, Quaternion rotation)
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

		// �ش� name�� ť�� ����ִٸ� ���� �߰� 
		if (poolQueue.Count <= 0)
		{
			var obj = CreateNewItem(name, viewID, ItemManager.GetItemByName(name).gameObject);

			if (obj != null)
            {
				ArrangePool(obj);
			}
			else
            {
				throw new Exception($"Item with name {name} doesn't exist in itemDictionary.");
            }
		}

		// ť�� ������Ʈ�� �����Ѵٸ� ������ ��� 
		GameObject objToSpawn = poolQueue.Dequeue();
		objToSpawn.transform.position = position;
		objToSpawn.transform.rotation = rotation;
		objToSpawn.SetActive(true);

		return objToSpawn;
	}

	#endregion

	#region Pool Ŭ������ �̿��� GameObject ���� �Լ� 

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

		// �ش� name�� ť�� ����ִٸ� ���� �߰� 
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

		// ť�� ������Ʈ�� �����Ѵٸ� ������ ��� 
		GameObject objToSpawn = poolQueue.Dequeue();
		objToSpawn.transform.position = position;
		objToSpawn.transform.rotation = rotation;
		objToSpawn.SetActive(true);

		return objToSpawn;
	}

	#endregion

	// name�� ������ �̸��� ��� pool�� List ���·� ��ȯ
	public static List<GameObject> GetAllPools(string name)
	{
		if(Instance._poolDictionary.ContainsKey(name) == false)
        {
			throw new Exception($"Pool with name {name} doesn't exist.");
        }

		return Instance._spawnedObjects.FindAll(x => x.name == name);
	}

	// name�� ������ �̸��� ��� pool�� List ���·� ��ȯ(Generic) 
	public static List<T> GetAllPools<T>(string name) where T : Component
	{
		List<GameObject> objects = GetAllPools(name);

		if (objects[0].TryGetComponent(out T component) == false)
        {
			throw new Exception("Component is not found");
		}

		return objects.ConvertAll(x => x.GetComponent<T>());
	}

	// ���� Pool�� ������ ��ü �� viewID�� ��ġ�ϴ� ���ӿ�����Ʈ ��ȯ 
	public static GameObject GetItemByViewID(int viewID)
    {
		return Instance._itemPoolDictionary[viewID];
    }

	// ����� ���� ���� ������Ʈ�� Pool�� ��ȯ 
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

	/// <summary>
	/// Json�� ���� ������ �Ľ� �� �� �����ۺ� Pool ���� 
	/// </summary>
	/// <param name="name"></param>
	/// <param name="item"></param>
	/// <exception cref="Exception"></exception>
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

	/// <summary>
	/// ItemManager�� ItemDictionary �� ��� �������� Pool�� stackSize��ŭ ���� 
	/// </summary>
	/// <exception cref="Exception"></exception>
	public static void CreateItemPoolsOnStart()
	{
		foreach (var item in ItemManager.GetItemDic())
		{
			Instance._poolDictionary.Add(item.Key, new Queue<GameObject>());

			if (Instance._poolDictionary.ContainsKey(item.Key) == false)
            {
				throw new Exception($"Pool with name {item.Key} doesn't exist");
			}

			for (int i = 0; i < item.Value.itemData.poolSize; i++)
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

			// ReturnToPool �ߺ����� �˻�
			if (Instance._poolDictionary[item.Key].Count != item.Value.itemData.poolSize)
				Debug.LogError($"{item.Key}�� ReturnToPool�� �ߺ��˴ϴ�.");
		}
	}

	/// <summary>
	/// ObjectPoolManageer�� pools �� ��� ������Ʈ�� ���� Pool ���� 
	/// </summary>
	/// <exception cref="Exception"></exception>
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

			// ReturnToPool �ߺ����� �˻�
			if (_poolDictionary[pool.name].Count != pool.size)
            {
				Debug.LogError($"{pool.name}�� ReturnToPool�� �ߺ��˴ϴ�");
			}
		}
	}

	// Item Ŭ������ ��ӹ��� GameObject ���� �Լ� 
	private GameObject CreateNewItem(string name, GameObject prefab)
	{
		var obj = Instantiate(prefab, transform);
		obj.name = name;
		obj.GetComponent<Item>().itemData.viewID = ++_viewID;
		_itemPoolDictionary.Add(obj.GetComponent<Item>().itemData.viewID, obj);

		// ��Ȱ��ȭ�� ReturnToPool�� �ϹǷ� Enqueue�� ��
		obj.SetActive(false);

		return obj;
	}

	private GameObject CreateNewItem(string name, int viewID, GameObject prefab)
	{
		var obj = Instantiate(prefab, transform);
		obj.name = name;
		obj.GetComponent<Item>().itemData.viewID = viewID;
		_itemPoolDictionary.Add(obj.GetComponent<Item>().itemData.viewID, obj);

		// ��Ȱ��ȭ�� ReturnToPool�� �ϹǷ� Enqueue�� ��
		obj.SetActive(false);

		return obj;
	}

	// Pool Ŭ������ �̿��� GameObject ���� �Լ�
	private GameObject CreateNewObject(string name, GameObject prefab)
	{
		var obj = Instantiate(prefab, transform);
		obj.name = name;

		// ��Ȱ��ȭ�� ReturnToPool�� �ϹǷ� Enqueue�� ��
		obj.SetActive(false); 

		return obj;
	}

	/// <summary>
	/// Pool�� �ִ� ��ü���� �̸��� ���� �����ϰ� ����Ʈ�� ���� 
	/// </summary>
	/// <param name="obj"></param>
	private void ArrangePool(GameObject obj)
	{
		// �߰��� ������Ʈ ��� ����
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

	public static int GetCurViewID()
    {
		return Instance._viewID;
    }

	public static void RaiseViewIDByOne()
    {
		Instance._viewID++;
    }
}

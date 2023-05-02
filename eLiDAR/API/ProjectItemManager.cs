using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eLiDAR.Models;


namespace eLiDAR.API
{
	public class EntityManager<T> where T: class, new()
	{
		readonly IRestService restService;

		public EntityManager(IRestService service)
		{
			restService = service;
		}

		public Task<List<T>> GetTasksAsync(DateTime? lastSyncTime, string plotId)
		{
			return restService.GetSyncDataAsync<T>(lastSyncTime, plotId);
		}

		public Task SaveTasksAsync(List<T> items, bool isNewItem = false)
		{
			return restService.PushSyncDataAsync<T>(items, isNewItem);
		}

		public Task DeleteTaskAsync(string itemId)
		{
			return restService.DeleteSyncDataAsync<T>(itemId);
		}
	}


	//public class PlotManager
	//{
	//	IRestService restService;

	//	public PlotManager(IRestService service)
	//	{
	//		restService = service;
	//	}

	//	public Task<List<PLOT>> GetTasksAsync(DateTime? lastSyncTime, string plotId)
	//	{
	//		return restService.GetSyncDataAsync<PLOT>(null, lastSyncTime, plotId);
	//	}

	//	public Task SaveTasksAsync(List<PLOT> items, bool isNewItem = false)
	//	{
	//		return restService.PushSyncDataAsync(items, isNewItem);
	//	}

	//	public Task DeleteTaskAsync(PLOT item)
	//	{
	//		return restService.DeleteSyncDataAsync<PLOT>(item.PLOTID);
	//	}
	//}


	//public class TreeManager
	//{
	//	IRestService restService;

	//	public TreeManager(IRestService service)
	//	{
	//		restService = service;
	//	}

	//	public Task<List<TREE>> GetTasksAsync(DateTime? lastSyncTime, string plotId)
	//	{
	//		return restService.GetSyncDataAsync<TREE>(null, lastSyncTime, plotId);
	//	}

	//	public Task SaveTasksAsync(List<TREE> items, bool isNewItem = false)
	//	{
	//		return restService.PushSyncDataAsync(items, isNewItem);
	//	}

	//	public Task DeleteTaskAsync(TREE item)
	//	{
	//		return restService.DeleteSyncDataAsync<TREE>(item.TREEID);
	//	}
	//}


	//public class StemmapManager
	//{
	//	IRestService restService;

	//	public StemmapManager(IRestService service)
	//	{
	//		restService = service;
	//	}

	//	public Task<List<STEMMAP>> GetTasksAsync(DateTime? lastSyncTime, string plotId)
	//	{
	//		return restService.GetSyncDataAsync<STEMMAP>(null, lastSyncTime, plotId);
	//	}

	//	public Task SaveTasksAsync(List<STEMMAP> items, bool isNewItem = false)
	//	{
	//		return restService.PushSyncDataAsync(items, isNewItem);
	//	}

	//	public Task DeleteTaskAsync(STEMMAP item)
	//	{
	//		return restService.DeleteSyncDataAsync<STEMMAP>(item.STEMMAPID);
	//	}
	//}

	//public class EcositeManager
	//{
	//	IRestService restService;

	//	public EcositeManager(IRestService service)
	//	{
	//		restService = service;
	//	}

	//	public Task<List<ECOSITE>> GetTasksAsync(DateTime? lastSyncTime, string plotId)
	//	{
	//		return restService.GetSyncDataAsync<ECOSITE>(null, lastSyncTime, plotId);
	//	}

	//	public Task SaveTasksAsync(List<ECOSITE> items, bool isNewItem = false)
	//	{
	//		return restService.PushSyncDataAsync(items, isNewItem);
	//	}

	//	public Task DeleteTaskAsync(ECOSITE item)
	//	{
	//		return restService.DeleteSyncDataAsync<ECOSITE>(item.ECOSITEID);
	//	}
	//}
	//public class SoilManager
	//{
	//	IRestService restService;

	//	public SoilManager(IRestService service)
	//	{
	//		restService = service;
	//	}

	//	public Task<List<SOIL>> GetTasksAsync(DateTime? lastSyncTime, string plotId)
	//	{
	//		return restService.GetSyncDataAsync<SOIL>(null, lastSyncTime, plotId);
	//	}

	//	public Task SaveTasksAsync(List<SOIL> items, bool isNewItem = false)
	//	{
	//		return restService.PushSyncDataAsync(items, isNewItem);
	//	}

	//	public Task DeleteTaskAsync(SOIL item)
	//	{
	//		return restService.DeleteSyncDataAsync<SOIL>(item.SOILID);
	//	}
	//}
	//public class SmalltreeManager
	//{
	//	IRestService restService;

	//	public SmalltreeManager(IRestService service)
	//	{
	//		restService = service;
	//	}

	//	public Task<List<SMALLTREE>> GetTasksAsync(DateTime? originalTime, DateTime? lastSyncTime, string plotId)
	//	{
	//		return restService.GetSyncDataAsync<SMALLTREE>(originalTime, lastSyncTime, plotId);
	//	}

	//	public Task SaveTasksAsync(List<SMALLTREE> items, bool isNewItem = false)
	//	{
	//		return restService.PushSyncDataAsync(items, isNewItem);
	//	}

	//	public Task DeleteTaskAsync(SMALLTREE item)
	//	{
	//		return restService.DeleteSyncDataAsync<SMALLTREE>(item.SMALLTREEID);
	//	}
	//}

	//public class SmalltreetallyManager
	//{
	//	IRestService restService;

	//	public SmalltreetallyManager(IRestService service)
	//	{
	//		restService = service;
	//	}

	//	public Task<List<SMALLTREETALLY>> GetTasksAsync(DateTime? originalTime, DateTime? lastSyncTime, string plotId)
	//	{
	//		return restService.GetSyncDataAsync<SMALLTREETALLY>(originalTime, lastSyncTime, plotId);
	//	}

	//	public Task SaveTasksAsync(List<SMALLTREETALLY> items, bool isNewItem = false)
	//	{
	//		return restService.PushSyncDataAsync(items, isNewItem);
	//	}

	//	public Task DeleteTaskAsync(SMALLTREETALLY item)
	//	{
	//		return restService.DeleteSyncDataAsync<SMALLTREETALLY>(item.SMALLTREETALLYID);
	//	}
	//}


	//public class VegetationManager
	//{
	//	IRestService restService;

	//	public VegetationManager(IRestService service)
	//	{
	//		restService = service;
	//	}

	//	public Task<List<VEGETATION>> GetTasksAsync(DateTime? originalTime, DateTime? lastSyncTime, string plotId)
	//	{
	//		return restService.GetSyncDataAsync<VEGETATION>(originalTime, lastSyncTime, plotId);
	//	}

	//	public Task SaveTasksAsync(List<VEGETATION> items, bool isNewItem = false)
	//	{
	//		return restService.PushSyncDataAsync(items, isNewItem);
	//	}

	//	public Task DeleteTaskAsync(VEGETATION item)
	//	{
	//		return restService.DeleteSyncDataAsync<VEGETATION>(item.VEGETATIONID);
	//	}
	//}

	//public class DeformityManager
	//{
	//	IRestService restService;

	//	public DeformityManager(IRestService service)
	//	{
	//		restService = service;
	//	}

	//	public Task<List<DEFORMITY>> GetTasksAsync(DateTime? originalTime, DateTime? lastSyncTime, string plotId)
	//	{
	//		return restService.GetSyncDataAsync<DEFORMITY>(originalTime, lastSyncTime, plotId);
	//	}

	//	public Task SaveTasksAsync(List<DEFORMITY> items, bool isNewItem = false)
	//	{
	//		return restService.PushSyncDataAsync(items, isNewItem);
	//	}

	//	public Task DeleteTaskAsync(DEFORMITY item)
	//	{
	//		return restService.DeleteSyncDataAsync<DEFORMITY>(item.DEFORMITYID);
	//	}
	//}

	//public class DWDManager
	//{
	//	IRestService restService;
	//	private static string tablename = "dwd";

	//	public DWDManager(IRestService service)
	//	{
	//		restService = service;
	//	}

	//	public Task<List<DWD>> GetTasksAsync(DateTime? originalTime, DateTime? lastSyncTime, string plotId)
	//	{
	//		return restService.GetSyncDataAsync<DWD>(originalTime, lastSyncTime, plotId);
	//	}

	//	public Task SaveTasksAsync(List<DWD> items, bool isNewItem = false)
	//	{
	//		return restService.PushSyncDataAsync(items, isNewItem);
	//	}

	//	public Task DeleteTaskAsync(DWD item)
	//	{
	//		return restService.DeleteSyncDataAsync<DWD>(item.DWDID);
	//	}
	//}


	//public class PhotoManager
	//{
	//	IRestService restService;
	//	private static string tablename = "photo";

	//	public PhotoManager(IRestService service)
	//	{
	//		restService = service;
	//	}

	//	public Task<List<PHOTO>> GetTasksAsync(DateTime? originalTime, DateTime? lastSyncTime, string plotId)
	//	{
	//		return restService.GetSyncDataAsync<PHOTO>(originalTime, lastSyncTime, plotId);
	//	}

	//	public Task SaveTasksAsync(List<PHOTO> items, bool isNewItem = false)
	//	{
	//		return restService.PushSyncDataAsync(items, isNewItem);
	//	}

	//	public Task DeleteTaskAsync(PHOTO item)
	//	{
	//		return restService.DeleteSyncDataAsync<PHOTO>(item.PHOTOID);
	//	}
	//}


	//public class PersonManager
	//{
	//	IRestService restService;
	//	private static string tablename = "person";

	//	public PersonManager(IRestService service)
	//	{
	//		restService = service;
	//	}

	//	public Task<List<PERSON>> GetTasksAsync(DateTime? lastSyncTime, string plotId)
	//	{
	//		return restService.GetSyncDataAsync<PERSON>(null, lastSyncTime, plotId);
	//	}

	//	public Task SaveTasksAsync(List<PERSON> items, bool isNewItem = false)
	//	{
	//		return restService.PushSyncDataAsync(items, isNewItem);
	//	}

	//	public Task DeleteTaskAsync(PERSON item)
	//	{
	//		return restService.DeleteSyncDataAsync<PERSON>(item.PERSONID);
	//	}
	//}


	//public class VegetationCensusManager
	//{
	//	IRestService restService;
	//	private static string tablename = "vegetationcensus";

	//	public VegetationCensusManager(IRestService service)
	//	{
	//		restService = service;
	//	}

	//	public Task<List<VEGETATIONCENSUS>> GetTasksAsync(DateTime? lastSyncTime, string plotId)
	//	{
	//		return restService.GetSyncDataAsync<VEGETATIONCENSUS>(null, lastSyncTime, plotId);
	//	}

	//	public Task SaveTasksAsync(List<VEGETATIONCENSUS> items, bool isNewItem = false)
	//	{
	//		return restService.PushSyncDataAsync(items, isNewItem);
	//	}

	//	public Task DeleteTaskAsync(VEGETATIONCENSUS item)
	//	{
	//		return restService.DeleteSyncDataAsync<VEGETATIONCENSUS>(item.VEGETATIONCENSUSID);
	//	}
	//}
}


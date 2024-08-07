using System;
using System.Collections.Generic;
using ClassicUO.Utility.Logging;
using ClassicUO.Network;

namespace Assistant
{
	internal delegate void PacketViewerCallback(Packet p, PacketHandlerEventArgs args);
	internal delegate void PacketFilterCallback(Packet p, PacketHandlerEventArgs args);

	internal class PacketHandlerEventArgs
	{
		internal bool Block { get; set; }

		internal PacketHandlerEventArgs()
		{
			Reinit();
		}

		internal void Reinit()
		{
			Block = false;
		}
	}

	internal class PacketHandler
	{
		private static Dictionary<int, List<PacketViewerCallback>> m_ClientViewers;
		private static Dictionary<int, List<PacketViewerCallback>> m_ServerViewers;

		private static Dictionary<int, List<PacketFilterCallback>> m_ClientFilters;
		private static Dictionary<int, List<PacketFilterCallback>> m_ServerFilters;

		static PacketHandler()
		{
			m_ClientViewers = new Dictionary<int, List<PacketViewerCallback>>();
			m_ServerViewers = new Dictionary<int, List<PacketViewerCallback>>();

			m_ClientFilters = new Dictionary<int, List<PacketFilterCallback>>();
			m_ServerFilters = new Dictionary<int, List<PacketFilterCallback>>();
		}

		internal static void RegisterClientToServerViewer(int packetID, PacketViewerCallback callback)
		{
			if (!m_ClientViewers.TryGetValue(packetID, out List<PacketViewerCallback> list) || list == null)
				m_ClientViewers[packetID] = list = new List<PacketViewerCallback>();
			list.Add(callback);
		}

		internal static void RegisterServerToClientViewer(int packetID, PacketViewerCallback callback)
		{
			if (!m_ServerViewers.TryGetValue(packetID, out List<PacketViewerCallback> list) || list == null)
				m_ServerViewers[packetID] = list = new List<PacketViewerCallback>();
			list.Add(callback);
		}

		internal static void RemoveClientToServerViewer(int packetID, PacketViewerCallback callback)
		{
			if (m_ClientViewers.TryGetValue(packetID, out List<PacketViewerCallback> list) && list != null)
				list.Remove(callback);
		}

		internal static void RemoveServerToClientViewer(int packetID, PacketViewerCallback callback)
		{
			if (m_ServerViewers.TryGetValue(packetID, out List<PacketViewerCallback> list) && list != null)
				list.Remove(callback);
		}

		internal static void RegisterClientToServerFilter(int packetID, PacketFilterCallback callback)
		{
			if (!m_ClientFilters.TryGetValue(packetID, out List<PacketFilterCallback> list) || list == null)
				m_ClientFilters[packetID] = list = new List<PacketFilterCallback>();
			list.Add(callback);
		}

		internal static void RegisterServerToClientFilter(int packetID, PacketFilterCallback callback)
		{
			if (!m_ServerFilters.TryGetValue(packetID, out List<PacketFilterCallback> list) || list == null)
				m_ServerFilters[packetID] = list = new List<PacketFilterCallback>();
			list.Add(callback);
		}

		internal static void RemoveClientToServerFilter(int packetID, PacketFilterCallback callback)
		{
			if (m_ClientFilters.TryGetValue(packetID, out List<PacketFilterCallback> list) && list != null)
				list.Remove(callback);
		}

		internal static void RemoveServerToClientFilter(int packetID, PacketFilterCallback callback)
		{
			if (m_ServerFilters.TryGetValue(packetID, out List<PacketFilterCallback> list) && list != null)
				list.Remove(callback);
		}

		internal static bool OnServerPacket(int id, Packet p, PacketAction pkta)
		{
			bool result = false;
			if ((pkta & PacketAction.Viewer) == PacketAction.Viewer)
			{
				if (m_ServerViewers.TryGetValue(id, out List<PacketViewerCallback> list) && list != null && list.Count > 0)
					result = ProcessViewers(list, p);
			}
			if((pkta & PacketAction.Filter) == PacketAction.Filter)
			{
				if (m_ServerFilters.TryGetValue(id, out List<PacketFilterCallback> list) && list != null && list.Count > 0)
					result |= ProcessFilters(list, p);
			}

			return result;
		}


		internal static bool OnClientPacket(int id, Packet p, PacketAction pkta)
		{
			bool result = false;
			if ((pkta & PacketAction.Viewer) == PacketAction.Viewer)
			{
				if (m_ClientViewers.TryGetValue(id, out List<PacketViewerCallback> list) && list != null && list.Count > 0)
					result = ProcessViewers(list, p);
			}
			if ((pkta & PacketAction.Filter) == PacketAction.Filter)
			{
				if (m_ClientFilters.TryGetValue(id, out List<PacketFilterCallback> list) && list != null && list.Count > 0)
					result |= ProcessFilters(list, p);
			}

			return result;
		}

		internal static PacketAction HasClientViewerFilter(int packetID)
		{
			PacketAction pkt = PacketAction.None;
			if (m_ClientViewers.TryGetValue(packetID, out List<PacketViewerCallback> flist) && flist != null && flist.Count > 0)
				pkt |= PacketAction.Viewer;
			if (m_ClientFilters.TryGetValue(packetID, out List<PacketFilterCallback> list) && list != null && list.Count > 0)
				pkt |= PacketAction.Filter;
			return pkt;
		}

		internal static PacketAction HasServerViewerFilter(int packetID)
		{
			PacketAction pkt = PacketAction.None;
			if (m_ServerViewers.TryGetValue(packetID, out List<PacketViewerCallback> list) && list != null && list.Count > 0)
				pkt |= PacketAction.Viewer;
			if (m_ServerFilters.TryGetValue(packetID, out List<PacketFilterCallback> flist) && flist != null && flist.Count > 0)
				pkt |= PacketAction.Filter;
			
			return pkt;
		}

		private static PacketHandlerEventArgs m_Args = new PacketHandlerEventArgs();
		private static bool ProcessViewers(List<PacketViewerCallback> list, Packet p)
		{
			m_Args.Reinit();

			if (list != null)
			{
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					p.MoveToData();
					list[i](p, m_Args);
				}
			}

			return m_Args.Block;
		}

		private static bool ProcessFilters(List<PacketFilterCallback> list, Packet p)
		{
			m_Args.Reinit();

			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					p.MoveToData();
					list[i](p, m_Args);
				}
			}

			return m_Args.Block;
		}
	}
}

using System;

namespace DeckSwipe.CardModel.Import {

	[Serializable]
	public class ProtoImage {

		public int id;
		public string url;
		public bool localFirst;

		public ProtoImage() {}

		public ProtoImage(
				int id,
				string url,
				bool localFirst) {
			this.id = id;
			this.url = url;
			this.localFirst = localFirst;
		}

	}

}

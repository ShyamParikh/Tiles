
namespace com.otm.manager
{
	public interface IInternetConnectionCheckCondition
	{
		void Init();
		bool CanCheckConnection();
	}
}
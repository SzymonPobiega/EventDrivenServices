using System.Threading.Tasks;

public interface IDelayGenerator
{
    Task WaitFor(string waitKey);
}
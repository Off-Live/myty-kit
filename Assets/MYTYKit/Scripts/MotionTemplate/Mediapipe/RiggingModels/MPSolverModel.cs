using MYTYKit.ThirdParty.MeFaMo;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public abstract class MPSolverModel : MotionTemplateBridge
    {
        protected MeFaMoSolver m_solver;

        public void SetSolver(MeFaMoSolver solver)
        {
            m_solver = solver;
        }
    }
}
namespace CircuitAR.AR
{
    public enum CircuitMode { Led, Series, Parallel }

    // Which circuit the student picked on the Workspace "SELECT A CIRCUIT" panel. Defaults to Led
    // so the original LED Circuit flow behaves identically for anyone who never touches Series/
    // Parallel. CircuitDemoController / SeriesCircuitController / ParallelCircuitController each
    // gate their own ShowARContent/HideARContent on this, so only the selected mode's 3D content
    // ever appears even though all three controllers listen to the same Vuforia tracking events.
    public static class CircuitModeState
    {
        public static CircuitMode Active = CircuitMode.Led;
    }
}

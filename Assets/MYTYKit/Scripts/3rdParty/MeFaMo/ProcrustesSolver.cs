using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace MYTYKit.ThirdParty.MeFaMo
{
    public class ProcrustesSolver
    {
        public static Matrix4x4 SolveWeightedOrthogonalProblem(Vector3[] sourcePoints, Vector3[] targetPoints, float[] pointWeights)
        {
            var sqrtWeights = ExtractSquareRoot(pointWeights);
            return InternalSolveWeightedOrthogonalProblem(sourcePoints, targetPoints, sqrtWeights);
        }

        static float[] ExtractSquareRoot(float[] pointWeights)
        {
            float[] sqrtWeights = new float[pointWeights.Length];
            for (int i = 0; i < sqrtWeights.Length; i++)
            {
                sqrtWeights[i] = Mathf.Sqrt(pointWeights[i]);
            }

            return sqrtWeights;
        }

        static Matrix4x4 InternalSolveWeightedOrthogonalProblem(Vector3[] sources, Vector3[] targets, float[] sqrtWeights)
        {
            Debug.Assert(sqrtWeights.Length == sources.Length && sqrtWeights.Length == targets.Length);
            var weightedSource = new Vector3[sources.Length];
            var weightedTarget = new Vector3[targets.Length];

            for (int i = 0; i < sqrtWeights.Length; i++)
            {
                weightedSource[i] = sources[i] * sqrtWeights[i];
                weightedTarget[i] = targets[i] * sqrtWeights[i];
            }

            var totalWeight = 0.0f;
            for (int i = 0; i < sqrtWeights.Length; i++)
            {
                totalWeight += sqrtWeights[i] * sqrtWeights[i];
            }

            var twiceWeightedSource = new Vector3[sources.Length];
            for (int i = 0; i < sqrtWeights.Length; i++)
            {
                twiceWeightedSource[i] = weightedSource[i] * sqrtWeights[i];
            }
        
            Vector3 sourceCenterOfMass = Vector3.zero;
            for (int i = 0; i < sqrtWeights.Length; i++)
            {
                sourceCenterOfMass += twiceWeightedSource[i];
            }

            sourceCenterOfMass /= totalWeight;

            var centeredWeightedSources = new Vector3[sources.Length];
            for (int i = 0; i < sqrtWeights.Length; i++)
            {
                centeredWeightedSources[i] =weightedSource[i] - sourceCenterOfMass * sqrtWeights[i];
            }

            var weightedTargetsMatrix = Matrix<float>.Build.Dense(3, targets.Length, (i, j) => weightedTarget[j][i]);
            var centeredWeightedSourcesMatrix =
                Matrix<float>.Build.Dense(3, sources.Length, (i, j) => centeredWeightedSources[j][i]);

            var designMatrix = weightedTargetsMatrix.TransposeAndMultiply(centeredWeightedSourcesMatrix);
            var rotation = ComputeOptimalRotation(designMatrix);

            var scale = ComputeOptimalScale(centeredWeightedSources, weightedSource, weightedTarget, rotation);
            var rotationAndScale = scale * rotation;

            var pointwiseDiffs = new Vector3[weightedTarget.Length];

            for (int i = 0; i < weightedTarget.Length; i++)
            {
                var index = i;
                var v = Vector<float>.Build.Dense(3, j => weightedSource[index][j]);
                var prod = rotationAndScale * v;
                pointwiseDiffs[i] = weightedTarget[i] - new Vector3(prod[0], prod[1], prod[2]);
            }

            var weightedPointwiseDiffs = new Vector3[weightedTarget.Length];
            var translation = Vector3.zero;
            for (int i = 0; i < weightedTarget.Length; i++)
            {
                weightedPointwiseDiffs[i] = pointwiseDiffs[i] * sqrtWeights[i];
                translation += weightedPointwiseDiffs[i];
            }

            translation /= totalWeight;

            return CombineTransformMatrix(rotationAndScale, translation);
        }

        static Matrix<float> ComputeOptimalRotation(Matrix<float> designMatrix)
        {
            var svd = designMatrix.Svd();
            var postrotation = svd.U;
            var prerotation = svd.VT;
            if (postrotation.Determinant() * prerotation.Determinant() < 0)
            {
                postrotation.SetColumn(2, postrotation.Column(2) * -1.0f);
            }

            return postrotation.Multiply(prerotation);
        }

        static float ComputeOptimalScale(Vector3[] centeredWeightedSources, Vector3[] weightedSources, Vector3[] weightedTargets, Matrix<float> rotation)
        {
            var rotatedCenteredWeightedSources = new Vector3[centeredWeightedSources.Length];
            for (int i = 0; i < centeredWeightedSources.Length; i++)
            {
                var index = i;
                Vector<float> v = Vector<float>.Build.Dense(3, (j) => centeredWeightedSources[index][j]);
                var prod = rotation * v;
                rotatedCenteredWeightedSources[i] = new Vector3(prod[0], prod[1], prod[2]);
            }

            var numerator = 0.0f;
            var denominator = 0.0f;
            for (int i = 0; i < weightedTargets.Length; i++)
            {
                numerator += Vector3.Dot(rotatedCenteredWeightedSources[i], weightedTargets[i]);
                denominator += Vector3.Dot(centeredWeightedSources[i], weightedSources[i]);
            }

            return numerator / denominator;
        }

        static Matrix4x4 CombineTransformMatrix(Matrix<float> rotaionAndScale, Vector3 translation)
        {
            var result = Matrix4x4.identity;
            for (int i = 0; i < 3; i++)
            {
                result.SetColumn(i,new Vector4(
                    rotaionAndScale[0,i],rotaionAndScale[1,i], rotaionAndScale[2,i],0));
            
            }
            result.SetColumn(3, new Vector4(translation[0],translation[1],translation[2],1));
            return result;
        }
    
    
    }
}

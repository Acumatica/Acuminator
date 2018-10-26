using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GlmNet;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.VertexBuffers;

namespace ForceGraph
{
    public class ForceGraphScene
    {
        public ForceGraph _forceGraph;

        private uint _pointsProgram;
        private uint _depDataTex;
        private uint _tbTex;
        private uint[] _texs;
        private uint[] _vaos;
        private uint[] _vbos;
        private int _projectionMatrixLoc;
        private int _viewMatrixLoc;
        private int _modelMatrixLoc;
        private int _timeStepLoc;
        private int _lightDirLoc;
        private int _nPointsLoc;
        private int _nPoints = 4;

        private uint _frameCount = 0;

        public void Init(OpenGL gl)
        {
            string[] varyings =
            {           
                "ft_vPosition", "ft_vVelocity", "ft_vColor", "ft_vIndex"
            };

            var vertexShaderSource = ManifestLoader.LoadTextFile(@"Shaders\points.vs.glsl");
            var fragmentShaderSource = ManifestLoader.LoadTextFile(@"Shaders\points.fs.glsl");
            uint program = gl.CreateProgram();
            CreateShader(gl, program, vertexShaderSource, OpenGL.GL_VERTEX_SHADER);
            CreateShader(gl, program, fragmentShaderSource, OpenGL.GL_FRAGMENT_SHADER);
            gl.TransformFeedbackVaryings(program, 4, varyings, OpenGL.GL_INTERLEAVED_ATTRIBS);
            gl.LinkProgram(program);
            gl.UseProgram(program);
            _pointsProgram = program;
            _projectionMatrixLoc = gl.GetUniformLocation(program, "projectionMatrix");
            _viewMatrixLoc = gl.GetUniformLocation(program, "viewMatrix");
            _modelMatrixLoc = gl.GetUniformLocation(program, "modelMatrix");
            _timeStepLoc = gl.GetUniformLocation(program, "timeStep");
            _lightDirLoc = gl.GetUniformLocation(program, "vLightDir");
            _nPointsLoc = gl.GetUniformLocation(program, "nPoints");


            gl.PointSize(64);
            gl.Enable(OpenGL.GL_PROGRAM_POINT_SIZE);
            gl.Enable(0x8861);

            _forceGraph = new ForceGraph();
            _forceGraph.LoadGraph("dependencies.txt");
            //CreateVertices(gl);
            CreateDacVertices(gl, "PX.Objects.SO.SOLine");
        }

        private uint CreateShader(OpenGL gl, uint program, string source, uint type)
        {
           
            uint sh = gl.CreateShader(type);
            gl.ShaderSource(sh, source);
            gl.CompileShader(sh);
            var sb = new System.Text.StringBuilder();
            gl.GetShaderInfoLog(sh, 4096, new IntPtr(), sb);
            gl.AttachShader(program, sh);
            gl.DeleteShader(sh);
            return sh;
        }

        public void CreateVertices(OpenGL gl)
        {
            var dacs = _forceGraph.GetDacs();
            int depCount = 600;

                _nPoints = depCount;

            int indexes = _nPoints * 3;

            byte[] depData = new byte[depCount * depCount];
            for (int i = 0; i < depCount * depCount; ++i)
            {
                depData[i] = 0x00;
            }

            
            
            foreach (var dac in dacs)
            {
                var d = _forceGraph.GetDependency(dac);
                int indexA = _forceGraph.GetIndex(dac);
                int rowA = indexA * _nPoints;
                int columnA = indexA;
                foreach (var dd in d.inDependency)
                {
                    int indexB = _forceGraph.GetIndex(dd.fromDac);
                    int rowB = indexB * _nPoints;
                    int columnB = indexB;
                    depData[rowA + columnB] = 0xff;
                    depData[rowB + columnA] = 0xff;
                }
                foreach (var dd in d.outDependency)
                {
                    int indexB = _forceGraph.GetIndex(dd.fromDac);
                    int rowB = indexB * _nPoints;
                    int columnB = indexB;
                    depData[rowA + columnB] = 0xff;
                    depData[rowB + columnA] = 0xff;
                }
            }
            var vertices = new float[indexes];
            var colors = new float[indexes];
            var velocties = new float[indexes];
            var dacIndexes = new float[indexes];
            float offset = 10.0f;
            Random r = new Random();
            for(int i = 0; i < indexes; i+=3)
            {
                vertices[i] = (float)r.NextDouble() * 2 * offset - offset; vertices[i+1] = (float)r.NextDouble() * 2 * offset - offset; vertices[i+2] = .0f;
                velocties[i] = .0f; velocties[i+1] = .0f; velocties[i+2] = .0f;
                colors[i] = 1.0f; colors[i+1] = 0.0f; colors[i+2] = 0.0f;
                if (i / 3 < dacs.Count)
                {
                    int dacIndex = _forceGraph.GetIndex(dacs[i / 3]);
                    dacIndexes[i] = BitConverter.ToSingle(BitConverter.GetBytes(dacIndex), 0);
                }
            }


            _texs = new uint[2];
            gl.GenTextures(2, _texs);
            _depDataTex = _texs[0];
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _depDataTex);
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_R8, depCount, depCount, 
                            0, OpenGL.GL_RED, OpenGL.GL_UNSIGNED_BYTE, depData);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_EDGE);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_EDGE);

            _tbTex = _texs[1];

            _vaos = new uint[2];
            gl.GenVertexArrays(2, _vaos);
            _vbos = new uint[2];
            gl.GenBuffers(2, _vbos);
            

            for (int i = 0; i < 2; ++i)
            {
                gl.BindBuffer(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, _vbos[i]);
                gl.BufferData(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, 
                    Marshal.SizeOf(typeof(float)) * 3 * 4 * indexes, new IntPtr(0), OpenGL.GL_DYNAMIC_COPY);

                float[] data = new float[indexes * 4];
                for (int j = 0; j < indexes; j+=3)
                {
                    int k = j * 4;
                    data[k] = vertices[j];
                    data[k+1] = vertices[j+1];
                    data[k+2] = vertices[j+2];

                    data[k+3] = velocties[j];
                    data[k+4] = velocties[j+1];
                    data[k+5] = velocties[j+2];

                    data[k + 6] = colors[j];
                    data[k + 7] = colors[j + 1];
                    data[k + 8] = colors[j + 2];

                    data[k + 9] = dacIndexes[j];
                    data[k + 10] = .0f;
                    data[k + 11] = .0f;
                }
                IntPtr gpuBuffer = gl.MapBuffer(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, OpenGL.GL_WRITE_ONLY);
                Marshal.Copy(data, 0, gpuBuffer, indexes * 4);
                gl.UnmapBuffer(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER);

                gl.BindVertexArray(_vaos[i]);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, _vbos[i]);

                gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 
                    Marshal.SizeOf(typeof(float)) * 3 * 4, new IntPtr(0));
                gl.VertexAttribPointer(1, 3, OpenGL.GL_FLOAT, false,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, (IntPtr)(Marshal.SizeOf(typeof(float)) * 3));
                gl.VertexAttribPointer(2, 3, OpenGL.GL_FLOAT, false,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, (IntPtr)(Marshal.SizeOf(typeof(float)) * 6));
                gl.VertexAttribIPointer(3, 3, OpenGL.GL_INT,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, (IntPtr)(Marshal.SizeOf(typeof(float)) * 9));

                gl.EnableVertexAttribArray(0);
                gl.EnableVertexAttribArray(1);
                gl.EnableVertexAttribArray(2);
                gl.EnableVertexAttribArray(3);

                gl.BindTexture(OpenGL.GL_TEXTURE_2D, _depDataTex);
                gl.BindTexture(OpenGL.GL_TEXTURE_BUFFER, _tbTex);
                gl.TexBuffer(OpenGL.GL_TEXTURE_BUFFER, OpenGL.GL_RGB32F, _vbos[i]);
            }
        }

        public void CreateDacVertices(OpenGL gl, string  targetDac)
        {
            var dep = _forceGraph.GetDependency(targetDac);
            Dictionary<string, int> str2int = new Dictionary<string, int>();
            Dictionary<int, string> int2str = new Dictionary<int, string>();
            str2int.Add(targetDac, 0);
            int2str.Add(0, targetDac);

            int counter = 1;
            foreach (var dd in dep.inDependency)
            {
                int t;
                if (str2int.TryGetValue(dd.fromDac, out t))
                    continue;
                str2int.Add(dd.fromDac, counter);
                int2str.Add(counter, dd.fromDac);
                ++counter;
            }
            foreach (var dd in dep.outDependency)
            {
                int t;
                if (str2int.TryGetValue(dd.fromDac, out t))
                    continue;
                int2str.Add(counter, dd.fromDac);
                ++counter;
            }


            _nPoints = str2int.Keys.Count;

            int indexes = _nPoints * 3;

            byte[] depData = new byte[_nPoints * _nPoints];
            for (int i = 0; i < _nPoints * _nPoints; ++i)
            {
                depData[i] = 0x00;
            }



            foreach (var dac in str2int.Keys)
            {
                var d = _forceGraph.GetDependency(dac);
                int indexA = str2int[dac];
                int rowA = indexA * _nPoints;
                int columnA = indexA;
                foreach (var dd in d.inDependency)
                {
                    int indexB = str2int[dd.fromDac];
                    int rowB = indexB * _nPoints;
                    int columnB = indexB;
                    depData[rowA + columnB] = 0xff;
                    depData[rowB + columnA] = 0xff;
                }
                foreach (var dd in d.outDependency)
                {
                    int indexB = str2int[dd.fromDac];
                    int rowB = indexB * _nPoints;
                    int columnB = indexB;
                    depData[rowA + columnB] = 0xff;
                    depData[rowB + columnA] = 0xff;
                }
            }
            var vertices = new float[indexes];
            var colors = new float[indexes];
            var velocties = new float[indexes];
            var dacIndexes = new float[indexes];
            float offset = 2.0f;
            Random r = new Random();
            for (int i = 0; i < indexes; i += 3)
            {
                vertices[i] = (float)r.NextDouble() * 2 * offset - offset; vertices[i + 1] = (float)r.NextDouble() * 2 * offset - offset; vertices[i + 2] = .0f;
                velocties[i] = .0f; velocties[i + 1] = .0f; velocties[i + 2] = .0f;
                colors[i] = 1.0f; colors[i + 1] = 0.0f; colors[i + 2] = 0.0f;
                dacIndexes[i] = BitConverter.ToSingle(BitConverter.GetBytes(i/3), 0);
            }


            _texs = new uint[2];
            gl.GenTextures(2, _texs);
            _depDataTex = _texs[0];
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _depDataTex);
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_R8, _nPoints, _nPoints,
                            0, OpenGL.GL_RED, OpenGL.GL_UNSIGNED_BYTE, depData);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_EDGE);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_EDGE);

            _tbTex = _texs[1];

            _vaos = new uint[2];
            gl.GenVertexArrays(2, _vaos);
            _vbos = new uint[2];
            gl.GenBuffers(2, _vbos);


            for (int i = 0; i < 2; ++i)
            {
                gl.BindBuffer(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, _vbos[i]);
                gl.BufferData(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER,
                    Marshal.SizeOf(typeof(float)) * 3 * 4 * indexes, new IntPtr(0), OpenGL.GL_DYNAMIC_COPY);

                float[] data = new float[indexes * 4];
                for (int j = 0; j < indexes; j += 3)
                {
                    int k = j * 4;
                    data[k] = vertices[j];
                    data[k + 1] = vertices[j + 1];
                    data[k + 2] = vertices[j + 2];

                    data[k + 3] = velocties[j];
                    data[k + 4] = velocties[j + 1];
                    data[k + 5] = velocties[j + 2];

                    data[k + 6] = colors[j];
                    data[k + 7] = colors[j + 1];
                    data[k + 8] = colors[j + 2];

                    data[k + 9] = dacIndexes[j];
                    data[k + 10] = .0f;
                    data[k + 11] = .0f;
                }
                IntPtr gpuBuffer = gl.MapBuffer(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, OpenGL.GL_WRITE_ONLY);
                Marshal.Copy(data, 0, gpuBuffer, indexes * 4);
                gl.UnmapBuffer(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER);

                gl.BindVertexArray(_vaos[i]);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, _vbos[i]);

                gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, new IntPtr(0));
                gl.VertexAttribPointer(1, 3, OpenGL.GL_FLOAT, false,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, (IntPtr)(Marshal.SizeOf(typeof(float)) * 3));
                gl.VertexAttribPointer(2, 3, OpenGL.GL_FLOAT, false,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, (IntPtr)(Marshal.SizeOf(typeof(float)) * 6));
                gl.VertexAttribIPointer(3, 3, OpenGL.GL_INT,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, (IntPtr)(Marshal.SizeOf(typeof(float)) * 9));

                gl.EnableVertexAttribArray(0);
                gl.EnableVertexAttribArray(1);
                gl.EnableVertexAttribArray(2);
                gl.EnableVertexAttribArray(3);

                gl.BindTexture(OpenGL.GL_TEXTURE_2D, _depDataTex);
                gl.BindTexture(OpenGL.GL_TEXTURE_BUFFER, _tbTex);
                gl.TexBuffer(OpenGL.GL_TEXTURE_BUFFER, OpenGL.GL_RGB32F, _vbos[i]);
            }
        }

        public void Render(OpenGL gl, mat4 pMat, mat4 vMat, mat4 mMat)
        {
            gl.UseProgram(_pointsProgram);
            gl.UniformMatrix4(_projectionMatrixLoc, 1, false, pMat.to_array());
            gl.UniformMatrix4(_viewMatrixLoc, 1, false, vMat.to_array());
            gl.UniformMatrix4(_modelMatrixLoc, 1, false, mMat.to_array());
            gl.Uniform3(_lightDirLoc, 0.0f, 0.0f, 1.0f);
            gl.Uniform1(_nPointsLoc, _nPoints);

            if (_frameCount % 2 == 1)
            {
                gl.BindVertexArray(_vaos[1]);
                gl.BindBufferBase(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, 0, _vbos[0]);
            }
            else
            {
                gl.BindVertexArray(_vaos[0]);
                gl.BindBufferBase(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, 0, _vbos[1]);
            }
            gl.BeginTransformFeedback(OpenGL.GL_POINTS);
            gl.DrawArrays(OpenGL.GL_POINTS, 0, _nPoints);
            gl.EndTransformFeedback();
            gl.BindVertexArray(0);

            ++_frameCount;
        }

        internal void CleanUp(OpenGL gl)
        {
            gl.UseProgram(0);
            gl.DeleteProgram(_pointsProgram);
            gl.DeleteVertexArrays(2, _vaos);
            gl.DeleteBuffers(2, _vbos);
            gl.DeleteTextures(2, _texs);
        }

    }
}

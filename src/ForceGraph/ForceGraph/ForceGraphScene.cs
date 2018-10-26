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
        private uint _linesProgram;
        private uint _depDataTex;
        private uint _tbTex;
        private uint[] _texs;
        private uint[] _vaos;
        private uint[] _vbos;

        private int _pprojectionMatrixLoc;
        private int _pviewMatrixLoc;
        private int _pmodelMatrixLoc;
        private int _ptimeStepLoc;
        private int _plightDirLoc;
        private int _pnPointsLoc;

        private int _lprojectionMatrixLoc;
        private int _lviewMatrixLoc;
        private int _lmodelMatrixLoc;
        private int _lnPointsLoc;

        private int _nPoints = 4;
        private OpenGL _gl;

        private uint _frameCount = 0;

        public void Init(OpenGL gl)
        {
            _gl = gl;

            CreatePointsProgram();
            CreateLinesProgram();

            _gl.PointSize(64);
            _gl.Enable(OpenGL.GL_PROGRAM_POINT_SIZE);
            _gl.Enable(0x8861);

            _forceGraph = new ForceGraph();
            _forceGraph.LoadGraph("dependencies.txt");
            //CreateVertices(gl);
            CreateDacVertices(_gl, "PX.Objects.SO.SOLine");
        }

        private void CreateLinesProgram()
        {
            var vertexShaderSource = ManifestLoader.LoadTextFile(@"Shaders\lines.vs.glsl");
            var fragmentShaderSource = ManifestLoader.LoadTextFile(@"Shaders\lines.fs.glsl");
            uint program = _gl.CreateProgram();
            CreateShader(_gl, program, vertexShaderSource, OpenGL.GL_VERTEX_SHADER);
            CreateShader(_gl, program, fragmentShaderSource, OpenGL.GL_FRAGMENT_SHADER);
            _gl.LinkProgram(program);
            _gl.UseProgram(program);
            _linesProgram = program;
            _lprojectionMatrixLoc = _gl.GetUniformLocation(program, "projectionMatrix");
            _lviewMatrixLoc = _gl.GetUniformLocation(program, "viewMatrix");
            _lmodelMatrixLoc = _gl.GetUniformLocation(program, "modelMatrix");
            _lnPointsLoc = _gl.GetUniformLocation(program, "nPoints");
        }

        private void CreatePointsProgram()
        {
            string[] varyings =
            {
                "ft_vPosition", "ft_vVelocity", "ft_vColor", "ft_vIndex"
            };

            var vertexShaderSource = ManifestLoader.LoadTextFile(@"Shaders\points.vs.glsl");
            var fragmentShaderSource = ManifestLoader.LoadTextFile(@"Shaders\points.fs.glsl");
            uint program = _gl.CreateProgram();
            CreateShader(_gl, program, vertexShaderSource, OpenGL.GL_VERTEX_SHADER);
            CreateShader(_gl, program, fragmentShaderSource, OpenGL.GL_FRAGMENT_SHADER);
            _gl.TransformFeedbackVaryings(program, 4, varyings, OpenGL.GL_INTERLEAVED_ATTRIBS);
            _gl.LinkProgram(program);
            _gl.UseProgram(program);
            _pointsProgram = program;
            _pprojectionMatrixLoc = _gl.GetUniformLocation(program, "projectionMatrix");
            _pviewMatrixLoc = _gl.GetUniformLocation(program, "viewMatrix");
            _pmodelMatrixLoc = _gl.GetUniformLocation(program, "modelMatrix");
            _ptimeStepLoc = _gl.GetUniformLocation(program, "timeStep");
            _plightDirLoc = _gl.GetUniformLocation(program, "vLightDir");
            _pnPointsLoc = _gl.GetUniformLocation(program, "nPoints");
        }

        private uint CreateShader(OpenGL gl, uint program, string source, uint type)
        {
           
            uint sh = _gl.CreateShader(type);
            _gl.ShaderSource(sh, source);
            _gl.CompileShader(sh);
            var sb = new System.Text.StringBuilder();
            _gl.GetShaderInfoLog(sh, 4096, new IntPtr(), sb);
            _gl.AttachShader(program, sh);
            _gl.DeleteShader(sh);
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
            _gl.GenTextures(2, _texs);
            _depDataTex = _texs[0];
            _gl.BindTexture(OpenGL.GL_TEXTURE_2D, _depDataTex);
            _gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_R8, depCount, depCount, 
                            0, OpenGL.GL_RED, OpenGL.GL_UNSIGNED_BYTE, depData);
            _gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            _gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            _gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_EDGE);
            _gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_EDGE);

            _tbTex = _texs[1];

            _vaos = new uint[2];
            _gl.GenVertexArrays(2, _vaos);
            _vbos = new uint[2];
            _gl.GenBuffers(2, _vbos);
            

            for (int i = 0; i < 2; ++i)
            {
                _gl.BindBuffer(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, _vbos[i]);
                _gl.BufferData(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, 
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
                IntPtr gpuBuffer = _gl.MapBuffer(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, OpenGL.GL_WRITE_ONLY);
                Marshal.Copy(data, 0, gpuBuffer, indexes * 4);
                _gl.UnmapBuffer(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER);

                _gl.BindVertexArray(_vaos[i]);
                _gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, _vbos[i]);

                _gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false, 
                    Marshal.SizeOf(typeof(float)) * 3 * 4, new IntPtr(0));
                _gl.VertexAttribPointer(1, 3, OpenGL.GL_FLOAT, false,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, (IntPtr)(Marshal.SizeOf(typeof(float)) * 3));
                _gl.VertexAttribPointer(2, 3, OpenGL.GL_FLOAT, false,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, (IntPtr)(Marshal.SizeOf(typeof(float)) * 6));
                _gl.VertexAttribIPointer(3, 3, OpenGL.GL_INT,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, (IntPtr)(Marshal.SizeOf(typeof(float)) * 9));

                _gl.EnableVertexAttribArray(0);
                _gl.EnableVertexAttribArray(1);
                _gl.EnableVertexAttribArray(2);
                _gl.EnableVertexAttribArray(3);

                _gl.BindTexture(OpenGL.GL_TEXTURE_2D, _depDataTex);
                _gl.BindTexture(OpenGL.GL_TEXTURE_BUFFER, _tbTex);
                _gl.TexBuffer(OpenGL.GL_TEXTURE_BUFFER, OpenGL.GL_RGB32F, _vbos[i]);
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
            _gl.GenTextures(2, _texs);
            _depDataTex = _texs[0];
            _gl.BindTexture(OpenGL.GL_TEXTURE_2D, _depDataTex);
            _gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_R8, _nPoints, _nPoints,
                            0, OpenGL.GL_RED, OpenGL.GL_UNSIGNED_BYTE, depData);
            _gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            _gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            _gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_EDGE);
            _gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_EDGE);

            _tbTex = _texs[1];

            _vaos = new uint[3];
            _gl.GenVertexArrays(3, _vaos);
            _vbos = new uint[3];
            _gl.GenBuffers(3, _vbos);


            for (int i = 0; i < 2; ++i)
            {
                _gl.BindBuffer(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, _vbos[i]);
                _gl.BufferData(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER,
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
                IntPtr gpuBuffer = _gl.MapBuffer(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, OpenGL.GL_WRITE_ONLY);
                Marshal.Copy(data, 0, gpuBuffer, indexes * 4);
                _gl.UnmapBuffer(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER);

                _gl.BindVertexArray(_vaos[i]);
                _gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, _vbos[i]);

                _gl.VertexAttribPointer(0, 3, OpenGL.GL_FLOAT, false,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, new IntPtr(0));
                _gl.VertexAttribPointer(1, 3, OpenGL.GL_FLOAT, false,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, (IntPtr)(Marshal.SizeOf(typeof(float)) * 3));
                _gl.VertexAttribPointer(2, 3, OpenGL.GL_FLOAT, false,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, (IntPtr)(Marshal.SizeOf(typeof(float)) * 6));
                _gl.VertexAttribIPointer(3, 3, OpenGL.GL_INT,
                    Marshal.SizeOf(typeof(float)) * 3 * 4, (IntPtr)(Marshal.SizeOf(typeof(float)) * 9));

                _gl.EnableVertexAttribArray(0);
                _gl.EnableVertexAttribArray(1);
                _gl.EnableVertexAttribArray(2);
                _gl.EnableVertexAttribArray(3);

                _gl.BindTexture(OpenGL.GL_TEXTURE_2D, _depDataTex);
                _gl.BindTexture(OpenGL.GL_TEXTURE_BUFFER, _tbTex);
                _gl.TexBuffer(OpenGL.GL_TEXTURE_BUFFER, OpenGL.GL_RGB32F, _vbos[i]);
            }

            _gl.BindVertexArray(_vaos[2]);
            _gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, _vbos[2]);
            var lineIndexes = new int[_nPoints * _nPoints * 2];
            int count = 0;
            for (int i = 0; i < _nPoints; ++i)
            {
                for (int j = 0; j < _nPoints; ++j)
                {
                    lineIndexes[count++] = i;
                    lineIndexes[count++] = j;
                }
            }
            GCHandle handle = GCHandle.Alloc(lineIndexes, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            var size = Marshal.SizeOf(typeof(int)) * lineIndexes.Count();
            _gl.BufferData(OpenGL.GL_ARRAY_BUFFER, size, ptr, OpenGL.GL_STATIC_DRAW);
            _gl.VertexAttribPointer(0, 2, OpenGL.GL_INT, false, 0, new IntPtr(0));
            _gl.EnableVertexAttribArray(0);

            _gl.BindTexture(OpenGL.GL_TEXTURE_2D, _depDataTex);
            _gl.BindTexture(OpenGL.GL_TEXTURE_BUFFER, _tbTex);
            _gl.TexBuffer(OpenGL.GL_TEXTURE_BUFFER, OpenGL.GL_RGB32F, _vbos[0]);
        }

        private void UsePointsProgram(mat4 pMat, mat4 vMat, mat4 mMat)
        {
            _gl.UseProgram(_pointsProgram);
            _gl.UniformMatrix4(_pprojectionMatrixLoc, 1, false, pMat.to_array());
            _gl.UniformMatrix4(_pviewMatrixLoc, 1, false, vMat.to_array());
            _gl.UniformMatrix4(_pmodelMatrixLoc, 1, false, mMat.to_array());
            _gl.Uniform3(_plightDirLoc, 0.0f, 0.0f, 1.0f);
            _gl.Uniform1(_pnPointsLoc, _nPoints);
        }

        private void UseLinesProgram(mat4 pMat, mat4 vMat, mat4 mMat)
        {
            _gl.UseProgram(_linesProgram);
            _gl.UniformMatrix4(_lprojectionMatrixLoc, 1, false, pMat.to_array());
            _gl.UniformMatrix4(_lviewMatrixLoc, 1, false, vMat.to_array());
            _gl.UniformMatrix4(_lmodelMatrixLoc, 1, false, mMat.to_array());
            _gl.Uniform1(_lnPointsLoc, _nPoints);
        }
        public void Render(OpenGL gl, mat4 pMat, mat4 vMat, mat4 mMat)
        {
            _gl = gl;

            UsePointsProgram(pMat, vMat, mMat);

            if (_frameCount % 2 == 1)
            {
                _gl.BindVertexArray(_vaos[1]);
                _gl.BindBufferBase(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, 0, _vbos[0]);
            }
            else
            {
                _gl.BindVertexArray(_vaos[0]);
                _gl.BindBufferBase(OpenGL.GL_TRANSFORM_FEEDBACK_BUFFER, 0, _vbos[1]);
            }
            _gl.BeginTransformFeedback(OpenGL.GL_POINTS);
            _gl.DrawArrays(OpenGL.GL_POINTS, 0, _nPoints);
            _gl.EndTransformFeedback();
            _gl.BindVertexArray(0);

            _gl.BindVertexArray(_vaos[2]);
            UseLinesProgram(pMat, vMat, mMat);
            _gl.DrawArrays(OpenGL.GL_LINES, 0, _nPoints);
            
            ++_frameCount;
        }



        internal void CleanUp(OpenGL gl)
        {
            _gl.UseProgram(0);
            _gl.DeleteProgram(_pointsProgram);
            _gl.DeleteVertexArrays(3, _vaos);
            _gl.DeleteBuffers(3, _vbos);
            _gl.DeleteTextures(2, _texs);
        }

    }
}

using System;
using System.Runtime.InteropServices;
using GlmNet;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.VertexBuffers;

namespace ForceGraph
{
    class ForceGraphScene
    {
        private ForceGraph _forceGraph;

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

            CreateVertices(gl);
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

        private void CreateVertices(OpenGL gl)
        {
            _forceGraph = new ForceGraph();
            _forceGraph.LoadGraph("dependencies.txt");

            var dacs = _forceGraph.GetDacs();

            int indexes = _nPoints * 3;

            var vertices = new float[indexes];
            var colors = new float[indexes];
            var velocties = new float[indexes];
            vertices[0] = -0.1f; vertices[1] = -0.5f; vertices[2] = .0f;
            velocties[0] = .0f; velocties[1] = .0f; velocties[2] = .0f;
            colors[0] = 1.0f; colors[1] = 1.0f; colors[2] = 1.0f;

            vertices[3] = -0.5f; vertices[4] = 0.5f; vertices[5] = 0.0f;
            velocties[3] = .0f; velocties[4] = .0f; velocties[5] = .0f;
            colors[3] = 1.0f; colors[4] = 0.0f; colors[5] = 0.0f;

            vertices[6] = 0.1f; vertices[7] = 0.5f; vertices[8] = 0.0f;
            velocties[6] = .0f; velocties[7] = .0f; velocties[8] = .0f;
            colors[6] = 0.0f; colors[7] = 1.0f; colors[8] = 0.0f;

            vertices[9] = 0.5f; vertices[10] = -0.5f; vertices[11] = .0f;
            velocties[9] = .0f; velocties[10] = .0f; velocties[11] = .0f;
            colors[9] = 0.0f; colors[10] = 0.0f; colors[11] = 1.0f;

            byte[] depData = new byte[]
            {
                0x00, 0xFF, 0x00, 0xFF,
                0xFF, 0x00, 0xFF, 0xFF,
                0x00, 0xFF, 0x00, 0xFF,
                0xFF, 0xFF, 0xFF, 0x00
            };
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
                int pi = 0;
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

                    data[k + 9] = BitConverter.ToSingle(BitConverter.GetBytes(pi++),0);
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
            gl.DrawArrays(OpenGL.GL_POINTS, 0, 4);
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

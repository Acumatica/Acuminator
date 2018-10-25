using System;
using GlmNet;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.VertexBuffers;

namespace ForceGraph
{
    class ForceGraphScene
    {
        private const uint _vPosition = 0;
        private const uint _vColor = 1;
        private ShaderProgram _shaderProgram;
        private VertexBufferArray _vba;
        public void Init(OpenGL gl)
        {
            var vertexShaderSource = ManifestLoader.LoadTextFile(@"Shaders\points.vs.glsl");
            var fragmentShaderSource = ManifestLoader.LoadTextFile(@"Shaders\points.fs.glsl");
            _shaderProgram = new ShaderProgram();
            _shaderProgram.Create(gl, vertexShaderSource, fragmentShaderSource, null);
            _shaderProgram.BindAttributeLocation(gl, _vPosition, "vPosition");
            _shaderProgram.BindAttributeLocation(gl, _vColor, "vColor");
            _shaderProgram.AssertValid(gl);

            gl.PointSize(64);
            gl.Enable(OpenGL.GL_PROGRAM_POINT_SIZE);
            gl.Enable(0x8861);

            CreateVertices(gl);
        }

        private void CreateVertices(OpenGL gl)
        {
            var vertices = new float[12];
            var colors = new float[12]; // Colors for our vertices  
            vertices[0] = -0.5f; vertices[1] = -0.5f; vertices[2] = -1.0f; // Bottom left corner  
            colors[0] = 1.0f; colors[1] = 1.0f; colors[2] = 1.0f; // Bottom left corner  
            vertices[3] = -0.5f; vertices[4] = 0.5f; vertices[5] = 0.0f; // Top left corner  
            colors[3] = 1.0f; colors[4] = 0.0f; colors[5] = 0.0f; // Top left corner  
            vertices[6] = 0.5f; vertices[7] = 0.5f; vertices[8] = 0.0f; // Top Right corner  
            colors[6] = 0.0f; colors[7] = 1.0f; colors[8] = 0.0f; // Top Right corner  
            vertices[9] = 0.5f; vertices[10] = -0.5f; vertices[11] = 1.0f; // Bottom right corner  
            colors[9] = 0.0f; colors[10] = 0.0f; colors[11] = 1.0f; // Bottom right corner  

            _vba = new VertexBufferArray();
            _vba.Create(gl);
            _vba.Bind(gl);

            var vertexBuffer = new VertexBuffer();
            vertexBuffer.Create(gl);
            vertexBuffer.Bind(gl);
            vertexBuffer.SetData(gl, 0, vertices, false, 3);

            var colorBuffer = new VertexBuffer();
            colorBuffer.Create(gl);
            colorBuffer.Bind(gl);
            colorBuffer.SetData(gl, 1, colors, false, 3);

            _vba.Unbind(gl);
        }

        public void Render(OpenGL gl, mat4 pMat, mat4 vMat, mat4 mMat)
        {
            _shaderProgram.Bind(gl);
            _shaderProgram.SetUniformMatrix4(gl, "projectionMatrix", pMat.to_array());
            _shaderProgram.SetUniformMatrix4(gl, "viewMatrix", vMat.to_array());
            _shaderProgram.SetUniformMatrix4(gl, "modelMatrix", mMat.to_array());
            _shaderProgram.SetUniform3(gl, "vLightDir", 0.0f, 0.0f, 1.0f);

            _vba.Bind(gl);

            gl.DrawArrays(OpenGL.GL_POINTS, 0, 4);

            _vba.Unbind(gl);
            _shaderProgram.Unbind(gl);
        }
    }
}

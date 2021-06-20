#version 330 core

in vec3 aPosition;
in vec2 aTexCoord;
in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 normal;
out vec3 fragPos;
out vec2 texCoord;

void main()
{
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
    texCoord = aTexCoord;
    fragPos = vec3(vec4(aPosition, 1.0) * model);
    normal = aNormal * mat3(transpose(inverse(model)));
}
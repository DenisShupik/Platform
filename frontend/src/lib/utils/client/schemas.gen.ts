// This file is auto-generated by @hey-api/openapi-ts

export const CategoryDtoSchema = {
    required: ['categoryId', 'created', 'createdBy', 'forumId', 'title'],
    type: 'object',
    properties: {
        categoryId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/CategoryId'
                }
            ],
            description: 'Идентификатор раздела'
        },
        forumId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/ForumId'
                }
            ],
            description: 'Идентификатор форума'
        },
        title: {
            allOf: [
                {
                    '$ref': '#/components/schemas/CategoryTitle'
                }
            ],
            description: 'Название раздела'
        },
        created: {
            type: 'string',
            description: 'Дата и время создания раздела',
            format: 'date-time'
        },
        createdBy: {
            allOf: [
                {
                    '$ref': '#/components/schemas/UserId'
                }
            ],
            description: 'Идентификатор пользователя, создавшего раздел'
        }
    },
    additionalProperties: false
} as const;

export const CategoryIdSchema = {
    pattern: '^(?!00000000-0000-0000-0000-000000000000$)',
    type: 'string',
    additionalProperties: false,
    format: 'uuid'
} as const;

export const CategoryNotFoundErrorSchema = {
    required: ['$type', 'categoryId'],
    type: 'object',
    properties: {
        '$type': {
            type: 'string',
            readOnly: true
        },
        categoryId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/CategoryId'
                }
            ]
        }
    },
    additionalProperties: false
} as const;

export const CategoryTitleSchema = {
    maxLength: 128,
    minLength: 3,
    pattern: '^(?!\\s*$).+',
    type: 'string',
    additionalProperties: false
} as const;

export const CreateCategoryRequestSchema = {
    required: ['forumId', 'title'],
    type: 'object',
    properties: {
        forumId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/ForumId'
                }
            ],
            description: 'Идентификатор форума'
        },
        title: {
            allOf: [
                {
                    '$ref': '#/components/schemas/CategoryTitle'
                }
            ],
            description: 'Название раздела'
        }
    },
    additionalProperties: false
} as const;

export const CreateForumRequestSchema = {
    required: ['title'],
    type: 'object',
    properties: {
        title: {
            allOf: [
                {
                    '$ref': '#/components/schemas/ForumTitle'
                }
            ],
            description: 'Название форума'
        }
    },
    additionalProperties: false
} as const;

export const CreatePostRequestBodySchema = {
    required: ['content'],
    type: 'object',
    properties: {
        content: {
            type: 'string'
        }
    },
    additionalProperties: false
} as const;

export const CreateThreadRequestSchema = {
    required: ['categoryId', 'title'],
    type: 'object',
    properties: {
        categoryId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/CategoryId'
                }
            ],
            description: 'Идентификатор категории'
        },
        title: {
            allOf: [
                {
                    '$ref': '#/components/schemas/ThreadTitle'
                }
            ],
            description: 'Название темы'
        }
    },
    additionalProperties: false
} as const;

export const ForumDtoSchema = {
    required: ['created', 'createdBy', 'forumId', 'title'],
    type: 'object',
    properties: {
        forumId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/ForumId'
                }
            ],
            description: 'Идентификатор форума'
        },
        title: {
            allOf: [
                {
                    '$ref': '#/components/schemas/ForumTitle'
                }
            ],
            description: 'Название форума'
        },
        created: {
            type: 'string',
            description: 'Дата и время создания форума',
            format: 'date-time'
        },
        createdBy: {
            allOf: [
                {
                    '$ref': '#/components/schemas/UserId'
                }
            ],
            description: 'Идентификатор пользователя, создавшего форум'
        }
    },
    additionalProperties: false
} as const;

export const ForumIdSchema = {
    pattern: '^(?!00000000-0000-0000-0000-000000000000$)',
    type: 'string',
    additionalProperties: false,
    format: 'uuid'
} as const;

export const ForumNotFoundErrorSchema = {
    required: ['$type', 'forumId'],
    type: 'object',
    properties: {
        '$type': {
            type: 'string',
            readOnly: true
        },
        forumId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/ForumId'
                }
            ]
        }
    },
    additionalProperties: false
} as const;

export const ForumTitleSchema = {
    maxLength: 64,
    minLength: 3,
    pattern: '^(?!\\s*$).+',
    type: 'string',
    additionalProperties: false
} as const;

export const GetCategoryThreadsRequestSortTypeSchema = {
    enum: [0],
    type: 'integer',
    format: 'int32'
} as const;

export const GetCategoryThreadsRequestSortTypeSortCriteriaSchema = {
    required: ['field', 'order'],
    type: 'object',
    properties: {
        field: {
            allOf: [
                {
                    '$ref': '#/components/schemas/GetCategoryThreadsRequestSortType'
                }
            ]
        },
        order: {
            allOf: [
                {
                    '$ref': '#/components/schemas/SortOrderType'
                }
            ]
        }
    },
    additionalProperties: false
} as const;

export const PostDtoSchema = {
    required: ['content', 'created', 'createdBy', 'postId', 'threadId'],
    type: 'object',
    properties: {
        postId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/PostId'
                }
            ],
            description: 'Идентификатор сообщения'
        },
        threadId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/ThreadId'
                }
            ],
            description: 'Идентификатор темы'
        },
        content: {
            type: 'string',
            description: 'Содержимое сообщения'
        },
        created: {
            type: 'string',
            description: 'Дата и время создания сообщения',
            format: 'date-time'
        },
        createdBy: {
            allOf: [
                {
                    '$ref': '#/components/schemas/UserId'
                }
            ],
            description: 'Идентификатор пользователя, создавшего сообщение'
        }
    },
    additionalProperties: false
} as const;

export const PostIdSchema = {
    minimum: 1,
    type: 'integer',
    additionalProperties: false,
    format: 'int64'
} as const;

export const SortOrderTypeSchema = {
    enum: [0, 1],
    type: 'integer',
    format: 'int32'
} as const;

export const SortTypeSchema = {
    enum: [0],
    type: 'integer',
    format: 'int32'
} as const;

export const SortTypeSortCriteriaSchema = {
    required: ['field', 'order'],
    type: 'object',
    properties: {
        field: {
            allOf: [
                {
                    '$ref': '#/components/schemas/SortType'
                }
            ]
        },
        order: {
            allOf: [
                {
                    '$ref': '#/components/schemas/SortOrderType'
                }
            ]
        }
    },
    additionalProperties: false
} as const;

export const ThreadDtoSchema = {
    required: ['categoryId', 'created', 'createdBy', 'postIdSeq', 'threadId', 'title'],
    type: 'object',
    properties: {
        threadId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/ThreadId'
                }
            ],
            description: 'Идентификатор темы'
        },
        postIdSeq: {
            type: 'integer',
            description: 'Последний использованный идентификатор сообщения',
            format: 'int64'
        },
        categoryId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/CategoryId'
                }
            ],
            description: 'Идентификатор раздела'
        },
        title: {
            allOf: [
                {
                    '$ref': '#/components/schemas/ThreadTitle'
                }
            ],
            description: 'Название темы'
        },
        created: {
            type: 'string',
            description: 'Дата и время создания темы',
            format: 'date-time'
        },
        createdBy: {
            allOf: [
                {
                    '$ref': '#/components/schemas/UserId'
                }
            ],
            description: 'Идентификатор пользователя, создавшего тему'
        }
    },
    additionalProperties: false
} as const;

export const ThreadIdSchema = {
    pattern: '^(?!00000000-0000-0000-0000-000000000000$)',
    type: 'string',
    additionalProperties: false,
    format: 'uuid'
} as const;

export const ThreadNotFoundErrorSchema = {
    required: ['$type', 'threadId'],
    type: 'object',
    properties: {
        '$type': {
            type: 'string',
            readOnly: true
        },
        threadId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/ThreadId'
                }
            ]
        }
    },
    additionalProperties: false
} as const;

export const ThreadTitleSchema = {
    maxLength: 128,
    minLength: 3,
    pattern: '^(?!\\s*$).+',
    type: 'string',
    additionalProperties: false
} as const;

export const UserDtoSchema = {
    required: ['createdAt', 'email', 'enabled', 'userId', 'username'],
    type: 'object',
    properties: {
        userId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/UserId'
                }
            ],
            description: 'Идентификатор пользователя'
        },
        username: {
            type: 'string',
            description: 'Логин пользователя'
        },
        email: {
            type: 'string',
            description: 'Электронная почта пользователя'
        },
        enabled: {
            type: 'boolean',
            description: 'Активна ли учетная запись пользователя'
        },
        createdAt: {
            type: 'string',
            description: 'Дата и время создания учетной записи пользователя',
            format: 'date-time'
        }
    },
    additionalProperties: false
} as const;

export const UserIdSchema = {
    pattern: '^(?!00000000-0000-0000-0000-000000000000$)',
    type: 'string',
    additionalProperties: false,
    format: 'uuid'
} as const;

export const UserNotFoundErrorSchema = {
    required: ['$type', 'userId'],
    type: 'object',
    properties: {
        '$type': {
            type: 'string',
            readOnly: true
        },
        userId: {
            allOf: [
                {
                    '$ref': '#/components/schemas/UserId'
                }
            ]
        }
    },
    additionalProperties: false
} as const;
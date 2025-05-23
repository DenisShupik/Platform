// This file is auto-generated by @hey-api/openapi-ts

import { z } from 'zod';

export const zCategoryId = z.string().uuid().regex(/^(?!00000000-0000-0000-0000-000000000000$)/);

export const zForumId = z.string().uuid().regex(/^(?!00000000-0000-0000-0000-000000000000$)/);

export const zCategoryTitle = z.string().min(3).max(128).regex(/^(?!\s*$).+/);

export const zUserId = z.string().uuid().regex(/^(?!00000000-0000-0000-0000-000000000000$)/);

export const zCategoryDto = z.object({
    categoryId: zCategoryId,
    forumId: zForumId,
    title: zCategoryTitle,
    createdBy: zUserId,
    createdAt: z.string().datetime()
});

export const zCategoryNotFoundError = z.object({
    '$type': z.string().readonly(),
    categoryId: zCategoryId
});

export const zCreateCategoryRequestBody = z.object({
    forumId: zForumId,
    title: zCategoryTitle
});

export const zForumTitle = z.string().min(3).max(64).regex(/^(?!\s*$).+/);

export const zCreateForumRequestBody = z.object({
    title: zForumTitle
});

export const zPostContent = z.string().min(2).max(1024).regex(/^(?!\s*$).+/);

export const zCreatePostRequestBody = z.object({
    content: zPostContent
});

export const zThreadTitle = z.string().min(3).max(128).regex(/^(?!\s*$).+/);

export const zCreateThreadRequestBody = z.object({
    categoryId: zCategoryId,
    title: zThreadTitle
});

export const zForumContainsFilter = z.unknown();

export const zForumDto = z.object({
    forumId: zForumId,
    title: zForumTitle,
    createdBy: zUserId,
    createdAt: z.string().datetime()
});

export const zForumNotFoundError = z.object({
    '$type': z.string().readonly(),
    forumId: zForumId
});

export const zGetCategoryThreadsRequestSortType = z.unknown();

export const zSortOrderType = z.unknown();

export const zGetCategoryThreadsRequestSortTypeSortCriteria = z.object({
    field: zGetCategoryThreadsRequestSortType,
    order: zSortOrderType
});

export const zThreadId = z.string().uuid().regex(/^(?!00000000-0000-0000-0000-000000000000$)/);

export const zPostId = z.coerce.bigint().gte(BigInt(1));

export const zNonPostAuthorError = z.object({
    '$type': z.string().readonly(),
    threadId: zThreadId,
    postId: zPostId
});

export const zNonThreadOwnerError = z.object({
    '$type': z.string().readonly(),
    threadId: zThreadId
});

export const zNotOwnerError = z.object({
    '$type': z.string().readonly()
});

export const zPostDto = z.object({
    postId: zPostId,
    threadId: zThreadId,
    content: zPostContent,
    createdAt: z.string().datetime(),
    createdBy: zUserId,
    updatedAt: z.string().datetime(),
    updatedBy: zUserId,
    rowVersion: z.number().int()
});

export const zPostNotFoundError = z.object({
    '$type': z.string().readonly(),
    threadId: zThreadId,
    postId: zPostId
});

export const zPostStaleError = z.object({
    '$type': z.string().readonly(),
    threadId: zThreadId,
    postId: zPostId,
    rowVersion: z.number().int()
});

export const zSortType = z.unknown();

export const zSortTypeSortCriteria = z.object({
    field: zSortType,
    order: zSortOrderType
});

export const zThreadStatus = z.unknown();

export const zThreadDto = z.object({
    threadId: zThreadId,
    categoryId: zCategoryId,
    title: zThreadTitle,
    createdBy: zUserId,
    createdAt: z.string().datetime(),
    nextPostId: zPostId,
    status: zThreadStatus
});

export const zThreadNotFoundError = z.object({
    '$type': z.string().readonly(),
    threadId: zThreadId
});

export const zUpdatePostRequestBody = z.object({
    content: zPostContent,
    rowVersion: z.number().int()
});

export const zUserDto = z.object({
    userId: zUserId,
    username: z.string(),
    email: z.string(),
    enabled: z.boolean(),
    createdAt: z.string().datetime()
});

export const zUserNotFoundError = z.object({
    '$type': z.string().readonly(),
    userId: zUserId
});

export const zGetCategoriesResponse = z.array(zCategoryDto);

export const zCreateCategoryResponse = zCategoryId;

export const zGetCategoryResponse = zCategoryDto;

export const zGetCategoriesPostsCountResponse = z.object({});

export const zGetCategoriesPostsLatestResponse = z.object({});

export const zGetCategoriesThreadsCountResponse = z.object({});

export const zGetCategoryThreadsResponse = z.array(zThreadDto);

export const zGetForumsCountResponse = z.coerce.bigint();

export const zGetForumsResponse = z.array(zForumDto);

export const zCreateForumResponse = zForumId;

export const zGetForumResponse = zForumDto;

export const zGetForumsCategoriesCountResponse = z.object({});

export const zGetForumsCategoriesLatestResponse = z.object({});

export const zGetPostsResponse = z.array(zPostDto);

export const zGetThreadsResponse = z.array(zThreadDto);

export const zCreateThreadResponse = zThreadId;

export const zGetThreadsCountResponse = z.coerce.bigint();

export const zGetThreadResponse = zThreadDto;

export const zGetThreadsPostsCountResponse = z.object({});

export const zGetThreadsPostsLatestResponse = z.object({});

export const zGetPostOrderResponse = z.coerce.bigint();

export const zCreatePostResponse = zPostId;

export const zGetUsersResponse = z.array(zUserDto);

export const zGetUserByIdResponse = zUserDto;

export const zGetUsersByIdsResponse = z.array(zUserDto);
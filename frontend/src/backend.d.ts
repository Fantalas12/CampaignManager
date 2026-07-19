import { type HttpAgentOptions, type ActorConfig, type Agent } from "@dfinity/agent";
import type { Principal } from "@dfinity/principal";
import { CreateActorOptions } from "declarations/backend";
import { _SERVICE } from "declarations/backend/backend.did.d.js";
export interface Some<T> {
    __kind__: "Some";
    value: T;
}
export interface None {
    __kind__: "None";
}
export type Option<T> = Some<T> | None;
export interface SessionPlayer {
    participant: Principal;
    sessionId: string;
}
export interface Note {
    id: string;
    creator: Principal;
    gmContent: string;
    name: string;
    tags: Array<string>;
    playerContent: string;
    inGameDateTime?: string;
    sessionId: string;
}
export interface LinkedNoteDetails {
    hasReadAccess: boolean;
    sessionName: string;
    noteId: string;
    tags: Array<string>;
    noteName: string;
    linkedNoteId: string;
    isOwner: boolean;
}
export interface Session {
    id: string;
    creator: Principal;
    date: string;
    name: string;
    campaignId: string;
    description: string;
}
export interface MetadataNote {
    id: string;
    content: string;
    owner: Principal;
    name: string;
    campaignId: string;
    linkedSessionNotes: Array<string>;
}
export interface Campaign {
    id: string;
    owner: Principal;
    name: string;
    description: string;
    inGameDateTime: string;
}
export interface Participant {
    principal: Principal;
    role: Role;
}
export interface Invitation {
    id: string;
    status: InvitationStatus;
    role: Role;
    recipient: Principal;
    campaignId: string;
    sender: Principal;
    timestamp: Time;
}
export interface RSVP {
    name: string;
    inviteCode: string;
    timestamp: Time;
    attending: boolean;
}
export interface NoteAccess {
    permissions: number;
    noteId: string;
    participant: Principal;
}
export type Time = bigint;
export interface InviteCode {
    created: Time;
    code: string;
    used: boolean;
}
export interface UserProfile {
    name: string;
}
export declare const createActor: (canisterId: string | Principal, options?: CreateActorOptions, processError?: (error: unknown) => never) => backendInterface;
export declare const canisterId: string;
export enum InvitationStatus {
    pending = "pending",
    rejected = "rejected",
    accepted = "accepted"
}
export enum Role {
    gm = "gm",
    player = "player",
    both = "both"
}
export enum UserRole {
    admin = "admin",
    user = "user",
    guest = "guest"
}
export interface backendInterface {
    acceptInvitation(id: string): Promise<void>;
    addParticipant(campaignId: string, participant: Principal, role: Role): Promise<void>;
    addSessionPlayer(sessionId: string, participant: Principal): Promise<void>;
    addTag(noteId: string, tag: string): Promise<void>;
    assignCallerUserRole(user: Principal, role: UserRole): Promise<void>;
    createCampaign(id: string, name: string, description: string, inGameDateTime: string): Promise<void>;
    createMetadataNote(id: string, name: string, content: string, linkedSessionNotes: Array<string>, campaignId: string): Promise<void>;
    createNote(id: string, sessionId: string, name: string, playerContent: string, gmContent: string, inGameDateTime: string | null, tags: Array<string>): Promise<void>;
    createSession(id: string, campaignId: string, name: string, description: string, date: string): Promise<void>;
    deleteCampaign(id: string): Promise<void>;
    deleteNote(id: string): Promise<void>;
    deleteSession(id: string): Promise<void>;
    generateInviteCode(): Promise<string>;
    getAllRSVPs(): Promise<Array<RSVP>>;
    getCallerUserProfile(): Promise<UserProfile | null>;
    getCallerUserRole(): Promise<UserRole>;
    getCampaignDetails(campaignId: string): Promise<Campaign | null>;
    getInviteCodes(): Promise<Array<InviteCode>>;
    getLinkedMetadataNotes(sessionNoteId: string): Promise<Array<MetadataNote>>;
    getLinkedNotes(noteId: string): Promise<Array<LinkedNoteDetails>>;
    getMetadataNoteDetails(metadataNoteId: string): Promise<MetadataNote | null>;
    getMetadataNotes(): Promise<Array<MetadataNote>>;
    getNoteAccess(noteId: string): Promise<Array<NoteAccess>>;
    getNotes(sessionId: string): Promise<Array<Note>>;
    getNotesByTag(tag: string): Promise<Array<Note>>;
    getOwnedCampaigns(): Promise<Array<Campaign>>;
    getParticipants(campaignId: string): Promise<Array<Participant>>;
    getParticipatedCampaigns(): Promise<Array<Campaign>>;
    getReceivedInvitations(): Promise<Array<Invitation>>;
    getSentInvitations(): Promise<Array<Invitation>>;
    getSessionPlayers(sessionId: string): Promise<Array<SessionPlayer>>;
    getSessions(campaignId: string): Promise<Array<Session>>;
    getUserCampaignTags(): Promise<Array<string>>;
    getUserProfile(user: Principal): Promise<UserProfile | null>;
    initializeAccessControl(): Promise<void>;
    isCallerAdmin(): Promise<boolean>;
    isUsernameAvailable(username: string): Promise<boolean>;
    leaveCampaign(campaignId: string): Promise<void>;
    linkMetadataNoteByName(sessionNoteId: string, metadataNoteName: string, campaignId: string): Promise<void>;
    linkNotes(noteId: string, targetNoteName: string): Promise<void>;
    manageNoteAccess(noteId: string, participant: Principal, permissions: number): Promise<void>;
    rejectInvitation(id: string): Promise<void>;
    removeNoteLink(noteId: string, linkedNoteId: string): Promise<void>;
    removeParticipant(campaignId: string, participant: Principal): Promise<void>;
    removeSessionPlayer(sessionId: string, participant: Principal): Promise<void>;
    removeTag(noteId: string, tag: string): Promise<void>;
    saveCallerUserProfile(profile: UserProfile): Promise<void>;
    sendInvitation(id: string, campaignId: string, recipientUsername: string, role: Role): Promise<void>;
    submitRSVP(name: string, attending: boolean, inviteCode: string): Promise<void>;
    unlinkMetadataNote(metadataNoteId: string, sessionNoteId: string): Promise<void>;
    updateCampaign(id: string, name: string, description: string, inGameDateTime: string): Promise<void>;
    updateNote(id: string, name: string, playerContent: string, gmContent: string, inGameDateTime: string | null, tags: Array<string>): Promise<void>;
    updateParticipantRole(campaignId: string, participant: Principal, newRole: Role): Promise<void>;
    updateSession(id: string, name: string, description: string, date: string): Promise<void>;
}


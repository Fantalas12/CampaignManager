import { type HttpAgentOptions, type ActorConfig, type Agent, type ActorSubclass } from "@dfinity/agent";
import type { Principal } from "@dfinity/principal";
import { backend as _backend, createActor as _createActor, canisterId as _canisterId, CreateActorOptions } from "declarations/backend";
import { _SERVICE } from "declarations/backend/backend.did.d.js";
export interface Some<T> {
    __kind__: "Some";
    value: T;
}
export interface None {
    __kind__: "None";
}
export type Option<T> = Some<T> | None;
function some<T>(value: T): Some<T> {
    return {
        __kind__: "Some",
        value: value
    };
}
function none(): None {
    return {
        __kind__: "None"
    };
}
function isNone<T>(option: Option<T>): option is None {
    return option.__kind__ === "None";
}
function isSome<T>(option: Option<T>): option is Some<T> {
    return option.__kind__ === "Some";
}
function unwrap<T>(option: Option<T>): T {
    if (isNone(option)) {
        throw new Error("unwrap: none");
    }
    return option.value;
}
function candid_some<T>(value: T): [T] {
    return [
        value
    ];
}
function candid_none<T>(): [] {
    return [];
}
function record_opt_to_undefined<T>(arg: T | null): T | undefined {
    return arg == null ? undefined : arg;
}
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
export function createActor(canisterId: string | Principal, options?: CreateActorOptions, processError?: (error: unknown) => never): backendInterface {
    const actor = _createActor(canisterId, options);
    return new Backend(actor, processError);
}
export const canisterId = _canisterId;
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
import type { Campaign as _Campaign, Invitation as _Invitation, InvitationStatus as _InvitationStatus, MetadataNote as _MetadataNote, Note as _Note, Participant as _Participant, Role as _Role, Time as _Time, UserProfile as _UserProfile, UserRole as _UserRole } from "declarations/backend/backend.did.d.ts";
class Backend implements backendInterface {
    private actor: ActorSubclass<_SERVICE>;
    constructor(actor?: ActorSubclass<_SERVICE>, private processError?: (error: unknown) => never){
        this.actor = actor ?? _backend;
    }
    async acceptInvitation(arg0: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.acceptInvitation(arg0);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.acceptInvitation(arg0);
            return result;
        }
    }
    async addParticipant(arg0: string, arg1: Principal, arg2: Role): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.addParticipant(arg0, arg1, to_candid_Role_n1(arg2));
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.addParticipant(arg0, arg1, to_candid_Role_n1(arg2));
            return result;
        }
    }
    async addSessionPlayer(arg0: string, arg1: Principal): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.addSessionPlayer(arg0, arg1);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.addSessionPlayer(arg0, arg1);
            return result;
        }
    }
    async addTag(arg0: string, arg1: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.addTag(arg0, arg1);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.addTag(arg0, arg1);
            return result;
        }
    }
    async assignCallerUserRole(arg0: Principal, arg1: UserRole): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.assignCallerUserRole(arg0, to_candid_UserRole_n3(arg1));
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.assignCallerUserRole(arg0, to_candid_UserRole_n3(arg1));
            return result;
        }
    }
    async createCampaign(arg0: string, arg1: string, arg2: string, arg3: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.createCampaign(arg0, arg1, arg2, arg3);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.createCampaign(arg0, arg1, arg2, arg3);
            return result;
        }
    }
    async createMetadataNote(arg0: string, arg1: string, arg2: string, arg3: Array<string>, arg4: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.createMetadataNote(arg0, arg1, arg2, arg3, arg4);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.createMetadataNote(arg0, arg1, arg2, arg3, arg4);
            return result;
        }
    }
    async createNote(arg0: string, arg1: string, arg2: string, arg3: string, arg4: string, arg5: string | null, arg6: Array<string>): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.createNote(arg0, arg1, arg2, arg3, arg4, to_candid_opt_n5(arg5), arg6);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.createNote(arg0, arg1, arg2, arg3, arg4, to_candid_opt_n5(arg5), arg6);
            return result;
        }
    }
    async createSession(arg0: string, arg1: string, arg2: string, arg3: string, arg4: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.createSession(arg0, arg1, arg2, arg3, arg4);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.createSession(arg0, arg1, arg2, arg3, arg4);
            return result;
        }
    }
    async deleteCampaign(arg0: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.deleteCampaign(arg0);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.deleteCampaign(arg0);
            return result;
        }
    }
    async deleteNote(arg0: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.deleteNote(arg0);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.deleteNote(arg0);
            return result;
        }
    }
    async deleteSession(arg0: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.deleteSession(arg0);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.deleteSession(arg0);
            return result;
        }
    }
    async generateInviteCode(): Promise<string> {
        if (this.processError) {
            try {
                const result = await this.actor.generateInviteCode();
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.generateInviteCode();
            return result;
        }
    }
    async getAllRSVPs(): Promise<Array<RSVP>> {
        if (this.processError) {
            try {
                const result = await this.actor.getAllRSVPs();
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getAllRSVPs();
            return result;
        }
    }
    async getCallerUserProfile(): Promise<UserProfile | null> {
        if (this.processError) {
            try {
                const result = await this.actor.getCallerUserProfile();
                return from_candid_opt_n6(result);
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getCallerUserProfile();
            return from_candid_opt_n6(result);
        }
    }
    async getCallerUserRole(): Promise<UserRole> {
        if (this.processError) {
            try {
                const result = await this.actor.getCallerUserRole();
                return from_candid_UserRole_n7(result);
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getCallerUserRole();
            return from_candid_UserRole_n7(result);
        }
    }
    async getCampaignDetails(arg0: string): Promise<Campaign | null> {
        if (this.processError) {
            try {
                const result = await this.actor.getCampaignDetails(arg0);
                return from_candid_opt_n9(result);
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getCampaignDetails(arg0);
            return from_candid_opt_n9(result);
        }
    }
    async getInviteCodes(): Promise<Array<InviteCode>> {
        if (this.processError) {
            try {
                const result = await this.actor.getInviteCodes();
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getInviteCodes();
            return result;
        }
    }
    async getLinkedMetadataNotes(arg0: string): Promise<Array<MetadataNote>> {
        if (this.processError) {
            try {
                const result = await this.actor.getLinkedMetadataNotes(arg0);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getLinkedMetadataNotes(arg0);
            return result;
        }
    }
    async getLinkedNotes(arg0: string): Promise<Array<LinkedNoteDetails>> {
        if (this.processError) {
            try {
                const result = await this.actor.getLinkedNotes(arg0);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getLinkedNotes(arg0);
            return result;
        }
    }
    async getMetadataNoteDetails(arg0: string): Promise<MetadataNote | null> {
        if (this.processError) {
            try {
                const result = await this.actor.getMetadataNoteDetails(arg0);
                return from_candid_opt_n10(result);
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getMetadataNoteDetails(arg0);
            return from_candid_opt_n10(result);
        }
    }
    async getMetadataNotes(): Promise<Array<MetadataNote>> {
        if (this.processError) {
            try {
                const result = await this.actor.getMetadataNotes();
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getMetadataNotes();
            return result;
        }
    }
    async getNoteAccess(arg0: string): Promise<Array<NoteAccess>> {
        if (this.processError) {
            try {
                const result = await this.actor.getNoteAccess(arg0);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getNoteAccess(arg0);
            return result;
        }
    }
    async getNotes(arg0: string): Promise<Array<Note>> {
        if (this.processError) {
            try {
                const result = await this.actor.getNotes(arg0);
                return from_candid_vec_n11(result);
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getNotes(arg0);
            return from_candid_vec_n11(result);
        }
    }
    async getNotesByTag(arg0: string): Promise<Array<Note>> {
        if (this.processError) {
            try {
                const result = await this.actor.getNotesByTag(arg0);
                return from_candid_vec_n11(result);
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getNotesByTag(arg0);
            return from_candid_vec_n11(result);
        }
    }
    async getOwnedCampaigns(): Promise<Array<Campaign>> {
        if (this.processError) {
            try {
                const result = await this.actor.getOwnedCampaigns();
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getOwnedCampaigns();
            return result;
        }
    }
    async getParticipants(arg0: string): Promise<Array<Participant>> {
        if (this.processError) {
            try {
                const result = await this.actor.getParticipants(arg0);
                return from_candid_vec_n15(result);
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getParticipants(arg0);
            return from_candid_vec_n15(result);
        }
    }
    async getParticipatedCampaigns(): Promise<Array<Campaign>> {
        if (this.processError) {
            try {
                const result = await this.actor.getParticipatedCampaigns();
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getParticipatedCampaigns();
            return result;
        }
    }
    async getReceivedInvitations(): Promise<Array<Invitation>> {
        if (this.processError) {
            try {
                const result = await this.actor.getReceivedInvitations();
                return from_candid_vec_n20(result);
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getReceivedInvitations();
            return from_candid_vec_n20(result);
        }
    }
    async getSentInvitations(): Promise<Array<Invitation>> {
        if (this.processError) {
            try {
                const result = await this.actor.getSentInvitations();
                return from_candid_vec_n20(result);
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getSentInvitations();
            return from_candid_vec_n20(result);
        }
    }
    async getSessionPlayers(arg0: string): Promise<Array<SessionPlayer>> {
        if (this.processError) {
            try {
                const result = await this.actor.getSessionPlayers(arg0);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getSessionPlayers(arg0);
            return result;
        }
    }
    async getSessions(arg0: string): Promise<Array<Session>> {
        if (this.processError) {
            try {
                const result = await this.actor.getSessions(arg0);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getSessions(arg0);
            return result;
        }
    }
    async getUserCampaignTags(): Promise<Array<string>> {
        if (this.processError) {
            try {
                const result = await this.actor.getUserCampaignTags();
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getUserCampaignTags();
            return result;
        }
    }
    async getUserProfile(arg0: Principal): Promise<UserProfile | null> {
        if (this.processError) {
            try {
                const result = await this.actor.getUserProfile(arg0);
                return from_candid_opt_n6(result);
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.getUserProfile(arg0);
            return from_candid_opt_n6(result);
        }
    }
    async initializeAccessControl(): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.initializeAccessControl();
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.initializeAccessControl();
            return result;
        }
    }
    async isCallerAdmin(): Promise<boolean> {
        if (this.processError) {
            try {
                const result = await this.actor.isCallerAdmin();
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.isCallerAdmin();
            return result;
        }
    }
    async isUsernameAvailable(arg0: string): Promise<boolean> {
        if (this.processError) {
            try {
                const result = await this.actor.isUsernameAvailable(arg0);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.isUsernameAvailable(arg0);
            return result;
        }
    }
    async leaveCampaign(arg0: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.leaveCampaign(arg0);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.leaveCampaign(arg0);
            return result;
        }
    }
    async linkMetadataNoteByName(arg0: string, arg1: string, arg2: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.linkMetadataNoteByName(arg0, arg1, arg2);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.linkMetadataNoteByName(arg0, arg1, arg2);
            return result;
        }
    }
    async linkNotes(arg0: string, arg1: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.linkNotes(arg0, arg1);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.linkNotes(arg0, arg1);
            return result;
        }
    }
    async manageNoteAccess(arg0: string, arg1: Principal, arg2: number): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.manageNoteAccess(arg0, arg1, arg2);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.manageNoteAccess(arg0, arg1, arg2);
            return result;
        }
    }
    async rejectInvitation(arg0: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.rejectInvitation(arg0);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.rejectInvitation(arg0);
            return result;
        }
    }
    async removeNoteLink(arg0: string, arg1: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.removeNoteLink(arg0, arg1);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.removeNoteLink(arg0, arg1);
            return result;
        }
    }
    async removeParticipant(arg0: string, arg1: Principal): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.removeParticipant(arg0, arg1);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.removeParticipant(arg0, arg1);
            return result;
        }
    }
    async removeSessionPlayer(arg0: string, arg1: Principal): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.removeSessionPlayer(arg0, arg1);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.removeSessionPlayer(arg0, arg1);
            return result;
        }
    }
    async removeTag(arg0: string, arg1: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.removeTag(arg0, arg1);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.removeTag(arg0, arg1);
            return result;
        }
    }
    async saveCallerUserProfile(arg0: UserProfile): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.saveCallerUserProfile(arg0);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.saveCallerUserProfile(arg0);
            return result;
        }
    }
    async sendInvitation(arg0: string, arg1: string, arg2: string, arg3: Role): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.sendInvitation(arg0, arg1, arg2, to_candid_Role_n1(arg3));
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.sendInvitation(arg0, arg1, arg2, to_candid_Role_n1(arg3));
            return result;
        }
    }
    async submitRSVP(arg0: string, arg1: boolean, arg2: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.submitRSVP(arg0, arg1, arg2);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.submitRSVP(arg0, arg1, arg2);
            return result;
        }
    }
    async unlinkMetadataNote(arg0: string, arg1: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.unlinkMetadataNote(arg0, arg1);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.unlinkMetadataNote(arg0, arg1);
            return result;
        }
    }
    async updateCampaign(arg0: string, arg1: string, arg2: string, arg3: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.updateCampaign(arg0, arg1, arg2, arg3);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.updateCampaign(arg0, arg1, arg2, arg3);
            return result;
        }
    }
    async updateNote(arg0: string, arg1: string, arg2: string, arg3: string, arg4: string | null, arg5: Array<string>): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.updateNote(arg0, arg1, arg2, arg3, to_candid_opt_n5(arg4), arg5);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.updateNote(arg0, arg1, arg2, arg3, to_candid_opt_n5(arg4), arg5);
            return result;
        }
    }
    async updateParticipantRole(arg0: string, arg1: Principal, arg2: Role): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.updateParticipantRole(arg0, arg1, to_candid_Role_n1(arg2));
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.updateParticipantRole(arg0, arg1, to_candid_Role_n1(arg2));
            return result;
        }
    }
    async updateSession(arg0: string, arg1: string, arg2: string, arg3: string): Promise<void> {
        if (this.processError) {
            try {
                const result = await this.actor.updateSession(arg0, arg1, arg2, arg3);
                return result;
            } catch (e) {
                this.processError(e);
                throw new Error("unreachable");
            }
        } else {
            const result = await this.actor.updateSession(arg0, arg1, arg2, arg3);
            return result;
        }
    }
}
export const backend: backendInterface = new Backend();
function from_candid_InvitationStatus_n23(value: _InvitationStatus): InvitationStatus {
    return from_candid_variant_n24(value);
}
function from_candid_Invitation_n21(value: _Invitation): Invitation {
    return from_candid_record_n22(value);
}
function from_candid_Note_n12(value: _Note): Note {
    return from_candid_record_n13(value);
}
function from_candid_Participant_n16(value: _Participant): Participant {
    return from_candid_record_n17(value);
}
function from_candid_Role_n18(value: _Role): Role {
    return from_candid_variant_n19(value);
}
function from_candid_UserRole_n7(value: _UserRole): UserRole {
    return from_candid_variant_n8(value);
}
function from_candid_opt_n10(value: [] | [_MetadataNote]): MetadataNote | null {
    return value.length === 0 ? null : value[0];
}
function from_candid_opt_n14(value: [] | [string]): string | null {
    return value.length === 0 ? null : value[0];
}
function from_candid_opt_n6(value: [] | [_UserProfile]): UserProfile | null {
    return value.length === 0 ? null : value[0];
}
function from_candid_opt_n9(value: [] | [_Campaign]): Campaign | null {
    return value.length === 0 ? null : value[0];
}
function from_candid_record_n13(value: {
    id: string;
    creator: Principal;
    gmContent: string;
    name: string;
    tags: Array<string>;
    playerContent: string;
    inGameDateTime: [] | [string];
    sessionId: string;
}): {
    id: string;
    creator: Principal;
    gmContent: string;
    name: string;
    tags: Array<string>;
    playerContent: string;
    inGameDateTime?: string;
    sessionId: string;
} {
    return {
        id: value.id,
        creator: value.creator,
        gmContent: value.gmContent,
        name: value.name,
        tags: value.tags,
        playerContent: value.playerContent,
        inGameDateTime: record_opt_to_undefined(from_candid_opt_n14(value.inGameDateTime)),
        sessionId: value.sessionId
    };
}
function from_candid_record_n17(value: {
    principal: Principal;
    role: _Role;
}): {
    principal: Principal;
    role: Role;
} {
    return {
        principal: value.principal,
        role: from_candid_Role_n18(value.role)
    };
}
function from_candid_record_n22(value: {
    id: string;
    status: _InvitationStatus;
    role: _Role;
    recipient: Principal;
    campaignId: string;
    sender: Principal;
    timestamp: _Time;
}): {
    id: string;
    status: InvitationStatus;
    role: Role;
    recipient: Principal;
    campaignId: string;
    sender: Principal;
    timestamp: Time;
} {
    return {
        id: value.id,
        status: from_candid_InvitationStatus_n23(value.status),
        role: from_candid_Role_n18(value.role),
        recipient: value.recipient,
        campaignId: value.campaignId,
        sender: value.sender,
        timestamp: value.timestamp
    };
}
function from_candid_variant_n19(value: {
    gm: null;
} | {
    player: null;
} | {
    both: null;
}): Role {
    return "gm" in value ? Role.gm : "player" in value ? Role.player : "both" in value ? Role.both : value;
}
function from_candid_variant_n24(value: {
    pending: null;
} | {
    rejected: null;
} | {
    accepted: null;
}): InvitationStatus {
    return "pending" in value ? InvitationStatus.pending : "rejected" in value ? InvitationStatus.rejected : "accepted" in value ? InvitationStatus.accepted : value;
}
function from_candid_variant_n8(value: {
    admin: null;
} | {
    user: null;
} | {
    guest: null;
}): UserRole {
    return "admin" in value ? UserRole.admin : "user" in value ? UserRole.user : "guest" in value ? UserRole.guest : value;
}
function from_candid_vec_n11(value: Array<_Note>): Array<Note> {
    return value.map((x)=>from_candid_Note_n12(x));
}
function from_candid_vec_n15(value: Array<_Participant>): Array<Participant> {
    return value.map((x)=>from_candid_Participant_n16(x));
}
function from_candid_vec_n20(value: Array<_Invitation>): Array<Invitation> {
    return value.map((x)=>from_candid_Invitation_n21(x));
}
function to_candid_Role_n1(value: Role): _Role {
    return to_candid_variant_n2(value);
}
function to_candid_UserRole_n3(value: UserRole): _UserRole {
    return to_candid_variant_n4(value);
}
function to_candid_opt_n5(value: string | null): [] | [string] {
    return value === null ? candid_none() : candid_some(value);
}
function to_candid_variant_n2(value: Role): {
    gm: null;
} | {
    player: null;
} | {
    both: null;
} {
    return value == Role.gm ? {
        gm: null
    } : value == Role.player ? {
        player: null
    } : value == Role.both ? {
        both: null
    } : value;
}
function to_candid_variant_n4(value: UserRole): {
    admin: null;
} | {
    user: null;
} | {
    guest: null;
} {
    return value == UserRole.admin ? {
        admin: null
    } : value == UserRole.user ? {
        user: null
    } : value == UserRole.guest ? {
        guest: null
    } : value;
}


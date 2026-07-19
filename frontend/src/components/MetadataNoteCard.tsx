import React, { useState, useEffect } from 'react';
import { MetadataNote } from '../backend';
import { useCheckMetadataNoteAccess } from '../hooks/useQueries';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Database, Link, User, Lock, Loader2 } from 'lucide-react';

interface MetadataNoteCardProps {
  metadataNote: MetadataNote;
  creatorName?: string;
  onMetadataNoteClick?: (metadataNote: MetadataNote) => void;
}

export default function MetadataNoteCard({ 
  metadataNote, 
  creatorName,
  onMetadataNoteClick 
}: MetadataNoteCardProps) {
  const [hasAccess, setHasAccess] = useState<boolean | null>(null);
  const [checkingAccess, setCheckingAccess] = useState(true);
  
  const checkAccess = useCheckMetadataNoteAccess();
  const { identity } = useInternetIdentity();

  const currentUserPrincipal = identity?.getPrincipal();
  const isOwner = metadataNote.owner.toString() === currentUserPrincipal?.toString();

  useEffect(() => {
    checkMetadataNoteAccess();
  }, [metadataNote.id, currentUserPrincipal]);

  const checkMetadataNoteAccess = async () => {
    if (!currentUserPrincipal) {
      setHasAccess(false);
      setCheckingAccess(false);
      return;
    }

    // If user is the owner, they always have access
    if (isOwner) {
      setHasAccess(true);
      setCheckingAccess(false);
      return;
    }

    try {
      const accessResult = await checkAccess.mutateAsync(metadataNote.id);
      setHasAccess(accessResult);
    } catch (error) {
      console.error('Failed to check metadata note access:', error);
      setHasAccess(false);
    } finally {
      setCheckingAccess(false);
    }
  };
  
  const handleClick = () => {
    if (hasAccess && onMetadataNoteClick) {
      onMetadataNoteClick(metadataNote);
    }
  };

  // Parse JSON content to show a preview
  const getContentPreview = () => {
    try {
      // Content is now a string directly from the backend
      const parsed = JSON.parse(metadataNote.content);
      const keys = Object.keys(parsed);
      if (keys.length === 0) return 'Empty JSON object';
      if (keys.length <= 3) {
        return `Keys: ${keys.join(', ')}`;
      }
      return `${keys.length} properties: ${keys.slice(0, 2).join(', ')}, ...`;
    } catch {
      return 'Invalid JSON content';
    }
  };

  return (
    <Card 
      className={`bg-white/10 backdrop-blur-sm border-white/20 transition-colors ${
        hasAccess 
          ? 'hover:bg-white/15 cursor-pointer group' 
          : 'opacity-75 cursor-not-allowed'
      }`}
      onClick={handleClick}
    >
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div className="flex items-center space-x-2 flex-1 min-w-0">
            <Database className="h-5 w-5 text-purple-400 flex-shrink-0" />
            <div className="min-w-0 flex-1">
              <CardTitle className={`text-lg truncate transition-colors ${
                hasAccess 
                  ? 'text-white group-hover:text-purple-200' 
                  : 'text-gray-400'
              }`}>
                {metadataNote.name}
                {isOwner && (
                  <span className="text-xs text-amber-400 ml-1">(Your note)</span>
                )}
              </CardTitle>
              <div className="flex items-center space-x-2 text-sm text-purple-300 mt-1">
                <User className="h-3 w-3" />
                <span className="truncate">{creatorName || 'Unknown'}</span>
              </div>
            </div>
          </div>
          <div className="flex items-center gap-2 flex-shrink-0">
            <Badge 
              variant="outline" 
              className="text-xs text-purple-300 border-purple-400/30 bg-purple-500/10"
            >
              Metadata
            </Badge>
            {checkingAccess && (
              <Loader2 className="h-4 w-4 animate-spin text-purple-400" />
            )}
            {!checkingAccess && !hasAccess && (
              <div className="flex items-center gap-1 text-xs text-red-400 bg-red-500/10 px-2 py-1 rounded border border-red-400/20">
                <Lock className="h-3 w-3" />
                <span>No access</span>
              </div>
            )}
          </div>
        </div>
      </CardHeader>
      
      <CardContent className="pt-0 space-y-3">
        <div>
          <CardDescription className={`text-sm ${
            hasAccess ? 'text-purple-200' : 'text-gray-400'
          }`}>
            {getContentPreview()}
          </CardDescription>
        </div>
        
        {metadataNote.linkedSessionNotes.length > 0 && (
          <div className="flex items-center space-x-2">
            <Link className="h-4 w-4 text-blue-400" />
            <span className={`text-xs ${
              hasAccess ? 'text-blue-300' : 'text-gray-400'
            }`}>
              Linked to {metadataNote.linkedSessionNotes.length} session note{metadataNote.linkedSessionNotes.length === 1 ? '' : 's'}
            </span>
          </div>
        )}
        
        <div className="pt-2 border-t border-white/10">
          <div className="flex items-center justify-between text-xs">
            <span className={hasAccess ? 'text-purple-300' : 'text-gray-400'}>
              {hasAccess ? 'Click to view JSON content' : 'Access restricted'}
            </span>
            <span className="text-purple-400">Metadata Note</span>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
